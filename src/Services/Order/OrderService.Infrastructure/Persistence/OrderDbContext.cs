using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Abstractions;
using OrderService.Domain.Aggregates;
using OrderService.Domain.Outbox;
using OrderService.Domain.Primitives;

namespace OrderService.Infrastructure.Persistence;

// DbContext implement IUnitOfWork — đây là implementation của interface ở Application
public sealed class OrderDbContext(
    DbContextOptions<OrderDbContext> options,
    IPublisher publisher) // MediatR publisher để dispatch domain events
    : DbContext(options), IUnitOfWork
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tự động scan và apply tất cả IEntityTypeConfiguration trong assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // IUnitOfWork implementation
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // 1. Thu thập domain events từ tất cả aggregates đang được track
        var aggregates = ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.GetDomainEvents().Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregates
            .SelectMany(a => a.GetDomainEvents())
            .ToList();

        // 2. Clear events trước khi save — tránh dispatch 2 lần nếu retry
        aggregates.ForEach(a => a.ClearDomainEvents());

        // 3. Lưu DB
        var result = await base.SaveChangesAsync(ct);

        // 4. Dispatch domain events SAU khi DB thành công
        // Nếu DB fail ở bước 3, events không bao giờ được dispatch
        foreach (var domainEvent in domainEvents)
            await publisher.Publish(domainEvent, ct);

        // ✅ Lưu lần 2 — persist OutboxMessages vào DB
        if (ChangeTracker.HasChanges())
            await base.SaveChangesAsync(ct);

        return result;
    }
}
