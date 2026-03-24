using FluentValidation;

namespace OrderService.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandValidator
    : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Lý do huỷ không được rỗng")
            .MaximumLength(200);
    }
}
