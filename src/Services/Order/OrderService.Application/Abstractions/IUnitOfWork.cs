namespace OrderService.Application.Abstractions;

// Interface ở Application — implementation ở Infrastructure
// Giúp SaveChanges + dispatch domain events trong 1 transaction
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
