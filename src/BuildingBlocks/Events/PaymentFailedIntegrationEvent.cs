// src/BuildingBlocks/Events/PaymentFailedIntegrationEvent.cs
using System.Text.Json.Serialization;

namespace BuildingBlocks.Events;

public sealed record PaymentFailedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid PaymentId,
    Guid OrderId,
    string Reason
) : IntegrationEvent(EventId, OccurredOn, nameof(PaymentFailedIntegrationEvent))
{
    [JsonConstructor]
    public PaymentFailedIntegrationEvent()
        : this(Guid.Empty, DateTime.UtcNow,
               Guid.Empty, Guid.Empty, string.Empty)
    { }

    public PaymentFailedIntegrationEvent(
        Guid paymentId, Guid orderId, string reason)
        : this(Guid.NewGuid(), DateTime.UtcNow,
               paymentId, orderId, reason)
    { }
}