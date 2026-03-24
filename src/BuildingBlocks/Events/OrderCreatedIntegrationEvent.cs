using System.Text.Json.Serialization;

namespace BuildingBlocks.Events;

// Event publish từ Order Service → Kafka → Payment Service consume
public sealed record OrderCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    List<OrderItemData> Items
) : IntegrationEvent(EventId, OccurredOn, nameof(OrderCreatedIntegrationEvent))
{
    [JsonConstructor]
    public OrderCreatedIntegrationEvent(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency,
        List<OrderItemData> items)
        : this(
            Guid.NewGuid(),
            DateTime.UtcNow,
            orderId,
            customerId,
            totalAmount,
            currency,
            items)
    { }
}

public sealed record OrderItemData(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice)
{
    // Cần cho deserialization
    [JsonConstructor]
    public OrderItemData() : this(Guid.Empty, string.Empty, 0, 0) { }
}