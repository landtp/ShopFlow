using OrderService.Application.Abstractions;

namespace OrderService.Application.Orders.Commands.CreateOrder;

// record → immutable, so sánh bằng value, ít boilerplate
public sealed record CreateOrderCommand(
    Guid CustomerId,
    string ShippingAddress,
    List<CreateOrderItemRequest> Items
) : ICommand<Guid>;  // trả về OrderId sau khi tạo

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    string Currency
);