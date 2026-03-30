using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentService.Domain.Entities;
using PaymentService.Domain.Exceptions;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure.Messaging;

public sealed class OrderCreatedServiceBusConsumer(
    ServiceBusClient client,
    IServiceScopeFactory scopeFactory,
    ILogger<OrderCreatedServiceBusConsumer> logger)
    : BackgroundService
{
    private const string TopicName = "order-created";
    private const string SubscriptionName = "payment-service";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Session Processor — đảm bảo ordering per OrderId
        var processor = client.CreateSessionProcessor(
            TopicName,
            SubscriptionName,
            new ServiceBusSessionProcessorOptions
            {
                // Số sessions xử lý song song
                // Mỗi session = 1 OrderId → độc lập nhau
                MaxConcurrentSessions = 10,
                MaxConcurrentCallsPerSession = 1,  // tuần tự per session

                // Tự renew lock — tránh lock expire khi xử lý lâu
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),

                // Manual complete — kiểm soát khi nào xoá message
                AutoCompleteMessages = false
            });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(ct);

        logger.LogInformation(
            "Payment Service Bus consumer started " +
            "topic: {Topic} subscription: {Sub}",
            TopicName, SubscriptionName);

        await Task.Delay(Timeout.Infinite, ct);
        await processor.StopProcessingAsync();
    }

    private async Task HandleMessageAsync(
        ProcessSessionMessageEventArgs args)
    {
        var ct = args.CancellationToken;
        var message = args.Message;

        logger.LogInformation(
            "Received message {MessageId} " +
            "session {SessionId} " +
            "delivery count {DeliveryCount}",
            message.MessageId,
            message.SessionId,
            message.DeliveryCount);

        try
        {
            var payload = message.Body.ToString();
            var evt = JsonSerializer
                .Deserialize<OrderCreatedIntegrationEvent>(payload);

            // Deserialize fail → Dead Letter ngay
            // Retry không có ý nghĩa — message bị corrupt
            if (evt is null)
            {
                logger.LogError(
                    "Failed to deserialize message {MessageId}",
                    message.MessageId);

                await args.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "DeserializationFailed",
                    deadLetterErrorDescription:
                        $"Cannot deserialize: {payload[..Math.Min(100, payload.Length)]}",
                    ct);
                return;
            }

            await ProcessPaymentAsync(evt, args, ct);

            // Complete → Service Bus xoá message
            await args.CompleteMessageAsync(message, ct);

            logger.LogInformation(
                "Completed message {MessageId} for Order {OrderId}",
                message.MessageId, evt.OrderId);
        }
        catch (BusinessException ex)
        {
            // Business error không thể recover → Dead Letter
            logger.LogError(ex,
                "Business error for message {MessageId}: {Error}",
                message.MessageId, ex.Message);

            await args.DeadLetterMessageAsync(
                message,
                deadLetterReason: "BusinessRuleViolation",
                deadLetterErrorDescription: ex.Message,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Transient error for message {MessageId} " +
                "delivery {DeliveryCount}",
                message.MessageId, message.DeliveryCount);

            // Max retries → Dead Letter
            if (message.DeliveryCount >= 3)
            {
                logger.LogWarning(
                    "Max retries exceeded for {MessageId} → DLQ",
                    message.MessageId);

                await args.DeadLetterMessageAsync(
                    message,
                    deadLetterReason: "MaxRetriesExceeded",
                    deadLetterErrorDescription: ex.Message,
                    ct);
            }
            else
            {
                await args.AbandonMessageAsync(
                    message,
                    propertiesToModify: null,
                    cancellationToken: ct);
            }
        }
    }

    private async Task ProcessPaymentAsync(
        OrderCreatedIntegrationEvent evt,
        ProcessSessionMessageEventArgs args,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<PaymentDbContext>();
        var eventBus = scope.ServiceProvider
            .GetRequiredService<ServiceBusEventBus>();

        // Idempotency check
        var alreadyProcessed = await db.ProcessedMessages
            .AnyAsync(m => m.MessageId == evt.EventId, ct);

        if (alreadyProcessed)
        {
            logger.LogInformation(
                "Message {EventId} already processed — skipping",
                evt.EventId);
            return;
        }

        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx =
                await db.Database.BeginTransactionAsync(ct);
            try
            {
                var payment = Payment.Create(
                    evt.OrderId,
                    evt.CustomerId,
                    evt.TotalAmount,
                    evt.Currency);

                var isSuccess = SimulatePayment(evt.TotalAmount);

                if (isSuccess)
                {
                    payment.MarkSuccess();

                    await eventBus.PublishAsync(
                        new PaymentCompletedIntegrationEvent(
                            payment.Id,
                            evt.OrderId,
                            evt.CustomerId,
                            evt.TotalAmount), ct);
                }
                else
                {
                    payment.MarkFailed("Insufficient funds");

                    await eventBus.PublishAsync(
                        new PaymentFailedIntegrationEvent(
                            payment.Id,
                            evt.OrderId,
                            "Insufficient funds"), ct);
                }

                await db.Payments.AddAsync(payment, ct);

                await db.ProcessedMessages.AddAsync(
                    new ProcessedMessage
                    {
                        MessageId = evt.EventId,
                        EventType = nameof(OrderCreatedIntegrationEvent),
                        ProcessedAt = DateTime.UtcNow
                    }, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                logger.LogInformation(
                    "Payment {Status} for Order {OrderId} " +
                    "via Service Bus",
                    payment.Status, evt.OrderId);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        });
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception,
            "Service Bus error source: {Source}",
            args.ErrorSource);
        return Task.CompletedTask;
    }

    private static bool SimulatePayment(decimal amount) =>
        Random.Shared.NextDouble() > 0.1;
}