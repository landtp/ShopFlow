using MediatR;

namespace OrderService.Domain.Primitives;

// Dùng INotification của MediatR để dispatch domain events
public interface IDomainEvent: INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

