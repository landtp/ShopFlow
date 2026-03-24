using BuildingBlocks.Events;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BuildingBlocks.Messaging;

public sealed class KafkaEventBus(
    IProducer<string, string> producer,
    ILogger<KafkaEventBus> logger)
    : IEventBus, IDisposable
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class
    {
        // Topic name = event type name (lowercase)
        // OrderCreatedIntegrationEvent → order-created
        var topic = GetTopicName(typeof(T).Name);

        var message = JsonSerializer.Serialize(integrationEvent);

        var kafkaMessage = new Message<string, string>
        {
            // Key = EventId để Kafka partition đúng
            Key = integrationEvent is IntegrationEvent evt
                    ? evt.EventId.ToString()
                    : Guid.NewGuid().ToString(),
            Value = message
        };

        try
        {
            var result = await producer.ProduceAsync(topic, kafkaMessage, ct);

            logger.LogInformation(
                "Published {EventType} to topic {Topic} partition {Partition} offset {Offset}",
                typeof(T).Name, topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex,
                "Failed to publish {EventType} to topic {Topic}",
                typeof(T).Name, topic);
            throw;
        }
    }

    // OrderCreatedIntegrationEvent → order-created
    private static string GetTopicName(string eventTypeName) =>
        eventTypeName
            .Replace("IntegrationEvent", "")
            .Replace("Event", "")
            .Aggregate(string.Empty, (acc, c) =>
                acc + (char.IsUpper(c) && acc.Length > 0 ? "-" : "") + char.ToLower(c))
            .Trim('-');

    public void Dispose() => producer.Dispose();
}