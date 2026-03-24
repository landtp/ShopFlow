using OrderService.Application.Abstractions;
using OrderService.Application.Orders.DTOs;

namespace OrderService.Application.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    Guid? CustomerId = null,
    int Page = 1,
    int PageSize = 20
) : IQuery<PagedResult<OrderDto>>;

// Generic paged result — dùng lại ở nhiều query
public sealed record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
}
