namespace BuildingBlocks.Events;

// Base class cho tất cả integration events — publish ra Kafka
public abstract record IntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    string EventType)
{
    protected IntegrationEvent() : this(
        Guid.NewGuid(),
        DateTime.UtcNow,
        string.Empty)
    { }
}