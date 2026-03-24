using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Persistence;
using System.Text.Json;

namespace PaymentService.Infrastructure.Messaging;

// BackgroundService — chạy liên tục trong background
public sealed class OrderCreatedConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedConsumer> logger)
    : BackgroundService
{
    private const string Topic = "order-created";
    private const string ConsumerGroup = "payment-service";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = ConsumerGroup,
            // Earliest — đọc từ đầu nếu consumer group mới
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Manual commit — chỉ commit sau khi xử lý thành công
            EnableAutoCommit = false,
            // Thêm dòng này — không throw lỗi khi topic chưa tồn tại
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) =>
                logger.LogError("Kafka error: {Error}", e.Reason))
            .Build();

        consumer.Subscribe(Topic);
        logger.LogInformation(
            "Payment Service subscribed to topic: {Topic}", Topic);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Poll message — timeout 1 giây
                var consumeResult = consumer.Consume(
                    TimeSpan.FromSeconds(1));

                if (consumeResult is null) continue;

                logger.LogInformation(
                    "Received message from topic {Topic} partition {Partition} offset {Offset}",
                    consumeResult.Topic,
                    consumeResult.Partition.Value,
                    consumeResult.Offset.Value);

                await ProcessMessageAsync(consumeResult.Message.Value, ct);

                // Manual commit — chỉ commit khi xử lý thành công
                consumer.Commit(consumeResult);
            }
            catch (ConsumeException ex)
            {
                logger.LogError(ex, "Consume error: {Error}", ex.Error.Reason);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing message");
                // Không commit → Kafka sẽ retry message này
            }
        }

        consumer.Close();
    }


    private async Task ProcessMessageAsync(string messageValue, CancellationToken ct)
    {
        var integrationEvent = JsonSerializer
            .Deserialize<OrderCreatedIntegrationEvent>(messageValue);

        if (integrationEvent is null) return;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

        // Idempotency check — ngoài transaction
        var alreadyProcessed = await db.ProcessedMessages
            .AnyAsync(m => m.MessageId == integrationEvent.EventId, ct);

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Message {EventId} already processed, skipping",
                integrationEvent.EventId);
            return;
        }

        // ✅ Fix: dùng CreateExecutionStrategy khi có EnableRetryOnFailure
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await db.Database.BeginTransactionAsync(ct);
            try
            {
                var payment = Payment.Create(
                    integrationEvent.OrderId,
                    integrationEvent.CustomerId,
                    integrationEvent.TotalAmount,
                    integrationEvent.Currency);

                var isSuccess = SimulatePayment(integrationEvent.TotalAmount);

                if (isSuccess)
                {
                    payment.MarkSuccess();
                    await eventBus.PublishAsync(
                        new PaymentCompletedIntegrationEvent(
                            payment.Id,
                            integrationEvent.OrderId,
                            integrationEvent.CustomerId,
                            integrationEvent.TotalAmount), ct);
                }
                else
                {
                    payment.MarkFailed("Insufficient funds");
                    await eventBus.PublishAsync(
                        new PaymentFailedIntegrationEvent(
                            payment.Id,
                            integrationEvent.OrderId,
                            "Insufficient funds"), ct);
                }

                await db.Payments.AddAsync(payment, ct);

                await db.ProcessedMessages.AddAsync(new ProcessedMessage
                {
                    MessageId = integrationEvent.EventId,
                    EventType = nameof(OrderCreatedIntegrationEvent),
                    ProcessedAt = DateTime.UtcNow
                }, ct);

                await db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                logger.LogInformation(
                    "Payment {Status} for Order {OrderId}",
                    payment.Status, integrationEvent.OrderId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                logger.LogError(ex,
                    "Failed to process payment for Order {OrderId}",
                    integrationEvent.OrderId);
                throw;
            }
        });
    }

    // Simulate: 90% success, 10% fail
    private static bool SimulatePayment(decimal amount) =>
        Random.Shared.NextDouble() > 0.1;
}