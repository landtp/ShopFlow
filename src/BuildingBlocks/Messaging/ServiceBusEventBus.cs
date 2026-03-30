using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BuildingBlocks.Events;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Messaging;

public sealed class ServiceBusEventBus(
    ServiceBusClient client,
    ILogger<ServiceBusEventBus> logger)
    : IEventBus, IAsyncDisposable
{
    private readonly Dictionary<string, ServiceBusSender>
        _senders = new();

    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken ct = default)
        where T : class
    {
        var topicName = GetTopicName(typeof(T).Name);
        var sender = await GetOrCreateSenderAsync(topicName);
        var payload = JsonSerializer.Serialize(integrationEvent);

        var message = new ServiceBusMessage(payload)
        {
            // MessageId cho Duplicate Detection
            MessageId = integrationEvent is IntegrationEvent evt
                          ? evt.EventId.ToString()
                          : Guid.NewGuid().ToString(),

            ContentType = "application/json",
            Subject = typeof(T).Name,

            // SessionId = OrderId → đảm bảo ordering
            // Tất cả messages của cùng Order xử lý tuần tự
            SessionId = GetSessionId(integrationEvent)
        };

        await sender.SendMessageAsync(message, ct);

        logger.LogInformation(
            "Published {EventType} to Service Bus topic {Topic} " +
            "session {SessionId}",
            typeof(T).Name, topicName, message.SessionId);
    }

    private async Task<ServiceBusSender> GetOrCreateSenderAsync(
        string topicName)
    {
        // Thread-safe sender cache
        await _lock.WaitAsync();
        try
        {
            if (!_senders.TryGetValue(topicName, out var sender))
            {
                sender = client.CreateSender(topicName);
                _senders[topicName] = sender;
            }
            return sender;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static string? GetSessionId<T>(T integrationEvent)
        where T : class
    {
        // Map từng event type sang SessionId
        // SessionId = OrderId → ordering per order
        return integrationEvent switch
        {
            OrderCreatedIntegrationEvent e => e.OrderId.ToString(),
            PaymentCompletedIntegrationEvent e => e.OrderId.ToString(),
            PaymentFailedIntegrationEvent e => e.OrderId.ToString(),
            _ => null
        };
    }

    // OrderCreatedIntegrationEvent → order-created
    private static string GetTopicName(string eventTypeName) =>
        string.Concat(eventTypeName
            .Replace("IntegrationEvent", "")
            .Select((c, i) => i > 0 && char.IsUpper(c)
                ? $"-{char.ToLower(c)}"
                : char.ToLower(c).ToString()))
        .Trim('-');

    public async ValueTask DisposeAsync()
    {
        foreach (var sender in _senders.Values)
            await sender.DisposeAsync();

        _lock.Dispose();
    }
}