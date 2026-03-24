using BuildingBlocks.Logging;
using BuildingBlocks.Messaging;
using Microsoft.EntityFrameworkCore;
using OrderService.Application;
using OrderService.Infrastructure;
using OrderService.Infrastructure.Persistence;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// ← Thêm Serilog
builder.AddSerilog("OrderService");

// Đăng ký từng layer
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddKafkaProducer(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// ← Request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} " +
        "responded {StatusCode} in {Elapsed:0.0000}ms";
});

// Configure the HTTP request pipeline.
// Auto migrate khi khởi động (dev only)
if (app.Environment.IsDevelopment())
{
    //app.MapOpenApi();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<OrderDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
// Global exception handler — map exceptions sang HTTP responses
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
        ?.Error;

    var (status, message) = ex switch
    {
        OrderService.Application.Exceptions.ValidationException ve =>
            (400, ve.Errors as object),
        OrderService.Application.Exceptions.NotFoundException nfe =>
            (404, new { error = nfe.Message } as object),
        OrderService.Domain.Primitives.DomainException de =>
            (422, new { error = de.Message } as object),
        _ =>
            (500, new { error = "Lỗi hệ thống" } as object)
    };

    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(message);
}));

app.MapControllers();


app.Run();

