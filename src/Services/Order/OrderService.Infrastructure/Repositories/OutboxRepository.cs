using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Outbox;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

internal sealed class OutboxRepository(OrderDbContext context)
    : IOutboxRepository
{
    public async Task<List<OutboxMessage>> GetUnprocessedAsync(
        int batchSize, CancellationToken ct = default) =>
        await context.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.Error == null)
            .OrderBy(m => m.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task AddAsync(
        OutboxMessage message, CancellationToken ct = default) =>
        await context.OutboxMessages.AddAsync(message, ct);

    public async Task UpdateAsync(
        OutboxMessage message, CancellationToken ct = default)
    {
        context.OutboxMessages.Update(message);
        await Task.CompletedTask;
    }
}