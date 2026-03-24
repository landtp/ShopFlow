using OrderService.Domain.Primitives;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates;

public sealed class OrderItem : Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero;

    // Tổng tiền của dòng này — computed, không lưu DB
    public Money SubTotal => UnitPrice * Quantity;

    // EF Core constructor
    private OrderItem() { }

    internal static OrderItem Create(
        Guid orderId,
        Guid productId,
        string productName,
        int quantity,
        Money unitPrice)
    {
        if (quantity <= 0)
            throw new DomainException("Số lượng phải lớn hơn 0");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    // internal — chỉ Order mới được gọi
    internal void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Số lượng tăng thêm phải lớn hơn 0");
        Quantity += quantity;
    }

}
