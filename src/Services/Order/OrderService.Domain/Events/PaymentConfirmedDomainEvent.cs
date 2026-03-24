using OrderService.Domain.Primitives;

namespace OrderService.Domain.Events;

public sealed record PaymentConfirmedDomainEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid PaymentId) : IDomainEvent
{
    public PaymentConfirmedDomainEvent(Guid orderId, Guid paymentId)
        : this(Guid.NewGuid(), DateTime.UtcNow, orderId, paymentId) { }
}
