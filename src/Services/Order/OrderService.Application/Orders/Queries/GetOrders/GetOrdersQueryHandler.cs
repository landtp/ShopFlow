using OrderService.Application.Abstractions;
using OrderService.Application.Orders.DTOs;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Orders.Queries.GetOrders;

internal sealed class GetOrdersQueryHandler(
    IOrderRepository orderRepository,
    ICacheService cacheService)
    : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(
        GetOrdersQuery query,
        CancellationToken ct)
    {
        // Cache key theo customerId + page
        var cacheKey = query.CustomerId.HasValue
            ? $"{CacheKeys.OrdersByCustomer(query.CustomerId.Value)}:page:{query.Page}"
            : $"orders:all:page:{query.Page}";

        // 1. Check cache trước
        var cached = await cacheService
            .GetAsync<PagedResult<OrderDto>>(cacheKey, ct);

        if (cached is not null)
            return cached; // Cache HIT → trả về ngay

        // 2. Cache MISS → query DB
        var orders = query.CustomerId.HasValue
            ? await orderRepository
                .GetByCustomerIdAsync(query.CustomerId.Value, ct)
            : await orderRepository.GetAllAsync(ct);

        var totalCount = orders.Count;

        // Paging
        var paged = orders
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(o => new OrderDto(
                o.Id,
                o.CustomerId,
                o.Status.Value,
                o.TotalAmount.Amount,
                o.TotalAmount.Currency,
                o.ShippingAddress,
                o.CreatedAt,
                o.UpdatedAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.ProductId, i.ProductName,
                    i.Quantity, i.UnitPrice.Amount,
                    i.SubTotal.Amount)).ToList()))
            .ToList();

        var result = new PagedResult<OrderDto>(
            paged, totalCount, query.Page, query.PageSize);

        // 3. Lưu vào cache 5 phút
        await cacheService.SetAsync(
            cacheKey, result,
            TimeSpan.FromMinutes(5), ct);

        return result;
    }
}