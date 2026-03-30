using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class ServiceBusDependencyInjection
{
    public static IServiceCollection AddServiceBusEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("ServiceBus")
            ?? throw new InvalidOperationException(
                "ServiceBus connection string chưa config");

        // ServiceBusClient là thread-safe → Singleton
        services.AddSingleton(
            new ServiceBusClient(connectionString));

        // Đăng ký tên để phân biệt với KafkaEventBus
        services.AddSingleton<ServiceBusEventBus>();

        return services;
    }
}