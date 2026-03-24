using OrderService.Application.Abstractions;
using OrderService.Application.Orders.DTOs;

namespace OrderService.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid OrderId)
    : IQuery<OrderDto>;
