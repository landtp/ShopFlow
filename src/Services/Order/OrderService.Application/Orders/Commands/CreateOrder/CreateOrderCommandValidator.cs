using FluentValidation;

namespace OrderService.Application.Orders.Commands.CreateOrder;

// Validation tách khỏi Handler — SRP
// ValidationBehavior sẽ tự động chạy trước Handler
public sealed class CreateOrderCommandValidator
    : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId không được rỗng");

        RuleFor(x => x.ShippingAddress)
            .NotEmpty()
            .WithMessage("Địa chỉ giao hàng không được rỗng")
            .MaximumLength(500)
            .WithMessage("Địa chỉ không được quá 500 ký tự");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order phải có ít nhất 1 sản phẩm");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty()
                .WithMessage("ProductId không được rỗng");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Số lượng phải lớn hơn 0");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0)
                .WithMessage("Đơn giá phải lớn hơn 0");
        });
    }
}