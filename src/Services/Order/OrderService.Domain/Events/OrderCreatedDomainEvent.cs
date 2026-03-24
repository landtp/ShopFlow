using OrderService.Domain.Primitives;

namespace OrderService.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency
) : IDomainEvent
{
    // Convenience constructor — EventId và OccurredOn tự sinh
    public OrderCreatedDomainEvent(Guid orderId, Guid customerId, decimal amount, string currency)
        : this(Guid.NewGuid(), DateTime.UtcNow, orderId, customerId, amount, currency) { }

}
