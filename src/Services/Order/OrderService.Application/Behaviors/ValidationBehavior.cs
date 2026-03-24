using FluentValidation;
using MediatR;
using ValidationException = OrderService.Application.Exceptions.ValidationException;

namespace OrderService.Application.Behaviors;

// Chỉ chạy khi có Validator — Command không có Validator thì bỏ qua
internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (!validators.Any())
            return await next();  // không có validator → bỏ qua

        var context = new ValidationContext<TRequest>(request);

        // Chạy tất cả validators song song
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);
        // → ValidationBehavior throw trước khi vào Handler
        // → Handler không bao giờ nhận invalid data

        return await next();
    }
}