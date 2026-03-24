using BuildingBlocks.Events;
using BuildingBlocks.Messaging;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Domain.Events;
using OrderService.Domain.Outbox;
using OrderService.Domain.Repositories;
using System.Text.Json;

namespace OrderService.Application.DomainEventHandlers;

// Lắng nghe OrderCreatedDomainEvent (internal)
// → Chuẩn bị publish OrderCreatedIntegrationEvent ra Kafka (bước sau)
internal sealed class OrderCreatedDomainEventHandler(
    IOrderRepository orderRepository,
    IOutboxRepository outboxRepository,
    ILogger<OrderCreatedDomainEventHandler> logger)
    : INotificationHandler<OrderCreatedDomainEvent>
{
    public async Task Handle(
        OrderCreatedDomainEvent notification,
        CancellationToken ct)
    {
        logger.LogInformation(
             "Handling OrderCreatedDomainEvent for Order {OrderId}",
             notification.OrderId);

        // Load full order để lấy items
        var order = await orderRepository
            .GetByIdAsync(notification.OrderId, ct);

        if (order is null)
        {
            logger.LogWarning(
                "Order {OrderId} not found when handling domain event",
                notification.OrderId);
            return;
        }

        // Chuyển Domain Event → Integration Event 
        var integrationEvent = new OrderCreatedIntegrationEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.Items.Select(i => new OrderItemData(
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice.Amount)).ToList());

        //Lưu vào Outbox — cùng transaction với Order
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = nameof(OrderCreatedIntegrationEvent),
            Payload = JsonSerializer.Serialize(integrationEvent),
            CreatedAt = DateTime.UtcNow
        };

        await outboxRepository.AddAsync(outboxMessage, ct);

        logger.LogInformation(
            "Saved {EventType} to Outbox for Order {OrderId}",
            nameof(OrderCreatedIntegrationEvent),
            notification.OrderId);
    }
}