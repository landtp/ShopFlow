using Azure.Messaging.ServiceBus;
using BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("PaymentDb"),
                sql => sql.EnableRetryOnFailure(3)));

        services.AddKafkaProducer(configuration);

        // Đăng ký Kafka consumer như Background Service
        services.AddHostedService<OrderCreatedConsumer>();
        
        // Service Bus (thêm mới — song song với Kafka)
        //var sbConnection = configuration
        //    .GetConnectionString("ServiceBus");

        //if (!string.IsNullOrEmpty(sbConnection))
        //{
        //    services.AddSingleton(
        //        new ServiceBusClient(sbConnection));

        //    services.AddSingleton<ServiceBusEventBus>();

        //    // Consumer với Sessions + DLQ
        //    services.AddHostedService
        //        <OrderCreatedServiceBusConsumer> ();

        //    // DLQ Monitor
        //    services.AddHostedService
        //        <DeadLetterQueueProcessor > ();

        //    Console.WriteLine("Service Bus consumers registered");
        //}

        return services;
    }
}