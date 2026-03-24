namespace OrderService.Application.Orders.DTOs;

// DTO là class thường — chỉ chứa data, không có logic
public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    string Status,
    decimal TotalAmount,
    string Currency,
    string ShippingAddress,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<OrderItemDto> Items
);

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);