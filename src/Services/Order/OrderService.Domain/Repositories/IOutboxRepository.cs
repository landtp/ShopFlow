using OrderService.Domain.Outbox;

namespace OrderService.Domain.Repositories;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetUnprocessedAsync(
        int batchSize, CancellationToken ct = default);

    Task AddAsync(
        OutboxMessage message, CancellationToken ct = default);

    Task UpdateAsync(
        OutboxMessage message, CancellationToken ct = default);
}
