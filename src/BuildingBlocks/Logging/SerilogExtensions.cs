using Microsoft.AspNetCore.Builder;
using Serilog;

namespace BuildingBlocks.Logging;

public static class SerilogExtensions
{
    public static WebApplicationBuilder AddSerilog(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Host.UseSerilog((context, services, config) =>
        {
            config
                // Đọc config từ appsettings
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)

                // Enrich thêm metadata
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", serviceName)

                // Console — format đẹp cho dev
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                    "{ServiceName} | {SourceContext} | " +
                    "{Message:lj}{NewLine}{Exception}")

                // File — rotate theo ngày
                .WriteTo.File(
                    path: $"logs/{serviceName}-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                    "[{Level:u3}] {ServiceName} | " +
                    "{SourceContext} | {Message:lj}{NewLine}{Exception}");
        });

        return builder;
    }
}