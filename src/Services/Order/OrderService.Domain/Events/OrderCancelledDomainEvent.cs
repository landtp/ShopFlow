using OrderService.Domain.Primitives;

namespace OrderService.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    string Reason) : IDomainEvent
{
    public OrderCancelledDomainEvent(Guid orderId, string reason)
        : this(Guid.NewGuid(), DateTime.UtcNow, orderId, reason) { }
}
