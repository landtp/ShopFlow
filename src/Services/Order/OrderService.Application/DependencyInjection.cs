using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Behaviors;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Tự động scan và đăng ký tất cả Handlers trong assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));

        // Tự động scan và đăng ký tất cả Validators
        //services.AddValidatorsFromAssembly(assembly);

        // Thứ tự behaviors = thứ tự chạy:
        // Logging → Validation → Transaction → Handler
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(TransactionBehavior<,>));

        return services;
    }
}
