using OrderService.Application.Abstractions;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Repositories;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Orders.Commands.CreateOrder;

internal sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    ICacheService cacheService)
    : ICommandHandler<CreateOrderCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateOrderCommand command,
        CancellationToken ct)
    {
        // Map từ command sang domain types
        var items = command.Items
            .Select(i => (
                i.ProductId,
                i.ProductName,
                i.Quantity,
                Money.Create(i.UnitPrice, i.Currency)))
            .ToList();

        // Domain tự validate — handler không cần check business rules
        var order = Order.Create(
            command.CustomerId,
            command.ShippingAddress,
            items);

        await orderRepository.AddAsync(order, ct);

        // SaveChanges + dispatch domain events trong 1 transaction
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate cache của customer này
        // → lần sau GET sẽ lấy data mới từ DB
        await cacheService.RemoveByPrefixAsync(
            CacheKeys.OrdersPrefix(command.CustomerId), ct);

        return order.Id;
    }
}
