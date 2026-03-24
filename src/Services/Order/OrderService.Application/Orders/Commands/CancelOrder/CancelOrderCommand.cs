using OrderService.Application.Abstractions;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(
    Guid OrderId,
    string Reason
) : ICommand;  // không trả về gì