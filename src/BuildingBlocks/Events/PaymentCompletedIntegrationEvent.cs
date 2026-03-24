// src/BuildingBlocks/Events/PaymentCompletedIntegrationEvent.cs
using System.Text.Json.Serialization;

namespace BuildingBlocks.Events;

public sealed record PaymentCompletedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount
) : IntegrationEvent(EventId, OccurredOn, nameof(PaymentCompletedIntegrationEvent))
{
    [JsonConstructor]
    public PaymentCompletedIntegrationEvent()
        : this(Guid.Empty, DateTime.UtcNow,
               Guid.Empty, Guid.Empty, Guid.Empty, 0)
    { }

    public PaymentCompletedIntegrationEvent(
        Guid paymentId, Guid orderId,
        Guid customerId, decimal amount)
        : this(Guid.NewGuid(), DateTime.UtcNow,
               paymentId, orderId, customerId, amount)
    { }
}