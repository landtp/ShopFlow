namespace OrderService.Application.Abstractions;

public static class CacheKeys
{
    // orders:customer:3fa85f64-...
    public static string OrdersByCustomer(Guid customerId) =>
        $"orders:customer:{customerId}";

    // orders:3fa85f64-...
    public static string OrderById(Guid orderId) =>
        $"orders:{orderId}";

    // Prefix để invalidate tất cả orders của 1 customer
    public static string OrdersPrefix(Guid customerId) =>
        $"orders:customer:{customerId}";
}