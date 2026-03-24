using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Messaging;

public static class KafkaDependencyInjection
{
    public static IServiceCollection AddKafkaProducer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? "localhost:9092";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            // Đảm bảo message không bị mất khi broker restart
            Acks = Acks.All,
            // Retry 3 lần nếu publish fail
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000,
            // Idempotent producer — không duplicate message
            EnableIdempotence = true
        };

        services.AddSingleton<IProducer<string, string>>(
            new ProducerBuilder<string, string>(config).Build());

        services.AddSingleton<IEventBus, KafkaEventBus>();

        return services;
    }
}