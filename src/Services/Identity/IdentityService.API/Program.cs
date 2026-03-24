using BuildingBlocks.Logging;
using IdentityService.Application;
using IdentityService.Infrastructure;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ← Thêm Serilog
builder.AddSerilog("IdentityService");

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ← Request logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} " +
        "responded {StatusCode} in {Elapsed:0.0000}ms";
});

// Exception handler — map exceptions → HTTP status codes
app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features
        .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()
        ?.Error;

    var (status, message) = ex switch
    {
        FluentValidation.ValidationException ve =>
            (400, (object)ve.Errors.Select(e => e.ErrorMessage)),
        IdentityService.Application.Exceptions.UnauthorizedException ue =>
            (401, new { error = ue.Message }),
        IdentityService.Application.Exceptions.ConflictException ce =>
            (409, new { error = ce.Message }),
        IdentityService.Application.Exceptions.NotFoundException nfe =>
            (404, new { error = nfe.Message }),
        _ =>
            (500, new { error = "Lỗi hệ thống" })
    };

    ctx.Response.StatusCode = status;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(message);
}));

// Auto migrate khi dev
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider
        .GetRequiredService<IdentityDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();