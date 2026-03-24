using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderService.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        //logger.LogInformation(
        //    "Bắt đầu xử lý {RequestName}: {@Request}",
        //    requestName, request);

        // Structured logging — properties có thể query được
        logger.LogInformation(
            "Executing {RequestName} {@Request}",
            requestName, request);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            //logger.LogInformation(
            //    "Hoàn thành {RequestName} trong {ElapsedMs}ms",
            //    requestName, stopwatch.ElapsedMilliseconds);

            logger.LogInformation(
               "Completed {RequestName} in {ElapsedMs}ms",
               requestName, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            //logger.LogError(ex,
            //    "Lỗi khi xử lý {RequestName} sau {ElapsedMs}ms",
            //    requestName, stopwatch.ElapsedMilliseconds);

            logger.LogError(ex,
                "Failed {RequestName} after {ElapsedMs}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
