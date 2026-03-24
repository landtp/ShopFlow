using OrderService.Domain.Events;
using OrderService.Domain.Primitives;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Aggregates;

public sealed class Order : AggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Money TotalAmount { get; private set; } = Money.Zero;
    public string ShippingAddress { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Optimistic concurrency — EF Core sẽ check field này khi UPDATE
    public byte[] Version { get; private set; } = [];

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    // EF Core constructor — private để bên ngoài không tạo Order "trống"
    // EF Core vẫn dùng được nhờ reflection
    private Order() { }

    // ── Factory method ─────────────────────────────────────────────
    public static Order Create(
        Guid customerId,
        string shippingAddress,
        List<(Guid ProductId, string ProductName, int Quantity, Money UnitPrice)> items)
    {
        if (items.Count == 0)
            throw new DomainException("Order phải có ít nhất 1 sản phẩm");

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new DomainException("Địa chỉ giao hàng không được rỗng");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            ShippingAddress = shippingAddress,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var (productId, productName, quantity, unitPrice) in items)
            order.AddItem(productId, productName, quantity, unitPrice);

        // Raise domain event — dispatcher sẽ xử lý sau khi SaveChanges
        order.RaiseDomainEvent(new OrderCreatedDomainEvent(
            order.Id, customerId, order.TotalAmount.Amount, order.TotalAmount.Currency));

        return order;
    }

    public void AddItem(Guid productId, string productName, int quantity, Money unitPrice)
    {
        if (!Status.Equals(OrderStatus.Pending))
        {
            throw new DomainException("Chỉ có thể thêm sản phẩm khi order đang Pending");
        }

        var existing = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existing is not null)
        {
            existing.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(OrderItem.Create(Id, productId, productName, quantity, unitPrice));
        }

        RecalculateTotal();
    }

    public void ConfirmPayment(Guid paymentId)
    {
        EnsureCanTransitionTo(OrderStatus.Confirmed);

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PaymentConfirmedDomainEvent(Id, paymentId));
    }

    public void Cancel(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Lý do huỷ không được rỗng");

        EnsureCanTransitionTo(OrderStatus.Cancelled);

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OrderCancelledDomainEvent(Id, reason));
    }

    public void Ship()
    {
        EnsureCanTransitionTo(OrderStatus.Shipped);
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Private helpers ────────────────────────────────────────────
    private void RecalculateTotal() =>
        TotalAmount = _items.Aggregate(
            Money.Zero,
            (sum, item) => sum + item.SubTotal);

    private void EnsureCanTransitionTo(OrderStatus next)
    {
        if (!Status.CanTransitionTo(next))
            throw new DomainException(
                $"Không thể chuyển từ {Status} sang {next}");
    }
}
