namespace BuildingBlocks.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken ct = default)
        where T : class;
}
