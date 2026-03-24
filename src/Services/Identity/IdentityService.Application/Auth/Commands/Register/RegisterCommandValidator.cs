using FluentValidation;

namespace IdentityService.Application.Auth.Commands.Register;

public sealed class RegisterCommandValidator
    : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được rỗng")
            .EmailAddress().WithMessage("Email không hợp lệ");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password không được rỗng")
            .MinimumLength(8).WithMessage("Password phải ít nhất 8 ký tự")
            .Matches("[A-Z]").WithMessage("Password phải có ít nhất 1 chữ hoa")
            .Matches("[0-9]").WithMessage("Password phải có ít nhất 1 số");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Tên không được rỗng")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Họ không được rỗng")
            .MaximumLength(50);
    }
}
