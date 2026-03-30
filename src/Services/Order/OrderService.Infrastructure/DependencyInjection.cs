using BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Caching;
using OrderService.Infrastructure.Messaging;
using OrderService.Infrastructure.Outbox;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;
using StackExchange.Redis;

namespace OrderService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core — đăng ký DbContext
        services.AddDbContext<OrderDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("OrderDb"),
                sql => sql.EnableRetryOnFailure(3)));

        // IUnitOfWork → OrderDbContext
        // Scoped vì DbContext là Scoped
        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<OrderDbContext>());

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        // Redis
        var redisConnection = configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnection);

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddSingleton<ICacheService, RedisCacheService>();

        // Background services
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<PaymentResultConsumer>(); // ← Saga consumer

        // Kafka producer
        services.AddKafkaProducer(configuration);

        // Service Bus (tạm comment — chưa có Azure account)
        // var sbConnection = configuration
        //     .GetConnectionString("ServiceBus");
        //
        // if (!string.IsNullOrEmpty(sbConnection))
        // {
        //     services.AddSingleton(
        //         new ServiceBusClient(sbConnection));
        //
        //     services.AddSingleton<ServiceBusEventBus>();
        //
        //     services.AddHostedService
        //         PaymentResultServiceBusConsumer>();
        // }

        return services;
    }
}