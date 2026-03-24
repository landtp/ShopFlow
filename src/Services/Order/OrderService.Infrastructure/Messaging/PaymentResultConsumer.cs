using System.Text.Json;
using BuildingBlocks.Events;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Messaging;

public sealed class PaymentResultConsumer(
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentResultConsumer> logger)
    : BackgroundService
{
    private const string ConsumerGroup = "order-payment-result";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        consumer.Subscribe(new[]
        {
            "payment-completed",
            "payment-failed"
        });

        logger.LogInformation(
            "Order Service subscribed to payment result topics");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                await ProcessPaymentResultAsync(
                    result.Topic, result.Message.Value, ct);

                consumer.Commit(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error consuming payment result");
            }
        }

        consumer.Close();
    }

    private async Task ProcessPaymentResultAsync(
        string topic, string payload, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<OrderDbContext>();

        if (topic == "payment-completed")
        {
            var evt = JsonSerializer
                .Deserialize<PaymentCompletedIntegrationEvent>(payload);
            if (evt is null) return;

            var order = await db.Orders
                .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);

            if (order is null) return;

            // Saga: Payment thành công → Order Confirmed
            order.ConfirmPayment(evt.PaymentId);
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Order {OrderId} confirmed after payment success",
                evt.OrderId);
        }
        else if (topic == "payment-failed")
        {
            var evt = JsonSerializer
                .Deserialize<PaymentFailedIntegrationEvent>(payload);
            if (evt is null) return;

            var order = await db.Orders
                .FirstOrDefaultAsync(o => o.Id == evt.OrderId, ct);

            if (order is null) return;

            // Saga compensating: Payment fail → Order Cancelled
            order.Cancel($"Payment failed: {evt.Reason}");
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Order {OrderId} cancelled due to payment failure: {Reason}",
                evt.OrderId, evt.Reason);
        }
    }
}