using OrderService.Application.Abstractions;
using OrderService.Application.Exceptions;
using OrderService.Application.Orders.DTOs;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository)
    : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(
        GetOrderByIdQuery query,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(query.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), query.OrderId);

        // Map sang DTO — Domain object không đi ra ngoài Application layer
        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Status.Value,
            order.TotalAmount.Amount,
            order.TotalAmount.Currency,
            order.ShippingAddress,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.Quantity,
                i.UnitPrice.Amount,
                i.SubTotal.Amount)).ToList());
    }
}
