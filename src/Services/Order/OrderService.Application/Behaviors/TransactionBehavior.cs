using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;

namespace OrderService.Application.Behaviors;

// Chỉ wrap Commands (có thay đổi DB), không wrap Queries
internal sealed class TransactionBehavior<TRequest, TResponse>(
   
    ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // Query không cần transaction — bỏ qua
        if (request is IQuery<TResponse>)
            return await next();

        logger.LogDebug("Bắt đầu transaction cho {Request}", typeof(TRequest).Name);

        try
        {
            var response = await next();
            // Handler đã gọi unitOfWork.SaveChangesAsync() bên trong
            // Transaction tự commit sau khi next() thành công
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction rollback cho {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}