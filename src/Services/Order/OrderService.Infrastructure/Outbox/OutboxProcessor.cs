using System.Text.Json;
using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Domain.Repositories;

namespace OrderService.Infrastructure.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger)
    : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("OutboxProcessor started");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxProcessor batch failed");
            }

            await Task.Delay(Interval, ct);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var outboxRepo = scope.ServiceProvider
            .GetRequiredService<IOutboxRepository>();
        var eventBus = scope.ServiceProvider
            .GetRequiredService<IEventBus>();
        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepo.GetUnprocessedAsync(BatchSize, ct);

        if (messages.Count == 0) return;

        logger.LogInformation(
            "OutboxProcessor processing {Count} messages", messages.Count);

        foreach (var message in messages)
        {
            try
            {
                var integrationEvent = message.EventType switch
                {
                    nameof(OrderCreatedIntegrationEvent) =>
                        (object?)JsonSerializer
                            .Deserialize<OrderCreatedIntegrationEvent>(
                                message.Payload),
                    _ => null
                };

                if (integrationEvent is null)
                {
                    message.Error = $"Unknown type: {message.EventType}";
                    await outboxRepo.UpdateAsync(message, ct);
                    continue;
                }

                await eventBus.PublishAsync(
                    (dynamic)integrationEvent, ct);

                message.ProcessedAt = DateTime.UtcNow;
                await outboxRepo.UpdateAsync(message, ct);

                logger.LogInformation(
                    "OutboxProcessor published {EventType} message {Id}",
                    message.EventType, message.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "OutboxProcessor failed message {Id}", message.Id);
                message.Error = ex.Message;
                await outboxRepo.UpdateAsync(message, ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
    }
}