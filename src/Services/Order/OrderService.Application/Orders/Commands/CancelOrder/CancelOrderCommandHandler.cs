using MediatR;
using OrderService.Application.Abstractions;
using OrderService.Application.Exceptions;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler(
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CancelOrderCommand>
{
    public async Task<Unit> Handle(
        CancelOrderCommand command,
        CancellationToken ct)
    {
        var order = await orderRepository.GetByIdAsync(command.OrderId, ct)
            ?? throw new NotFoundException(nameof(Order), command.OrderId);

        // Domain enforce business rule — handler không biết logic cancel
        order.Cancel(command.Reason);

        await orderRepository.UpdateAsync(order, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
