namespace OrderService.Domain.ValueObjects;

// Dùng enum-like record thay vì plain enum
// → có thể thêm behavior, dễ serialize, dễ validate transition
public sealed record OrderStatus
{
    public static readonly OrderStatus Pending = new("Pending");
    public static readonly OrderStatus Confirmed = new("Confirmed");
    public static readonly OrderStatus Shipped = new("Shipped");
    public static readonly OrderStatus Delivered = new("Delivered");
    public static readonly OrderStatus Cancelled = new("Cancelled");

    public string Value { get; }

    private OrderStatus(string value) => Value = value;

    // EF Core cần factory method để rehydrate từ DB
    public static OrderStatus From(string value) => value switch
    {
        "Pending" => Pending,
        "Confirmed" => Confirmed,
        "Shipped" => Shipped,
        "Delivered" => Delivered,
        "Cancelled" => Cancelled,
        _ => throw new ArgumentException($"OrderStatus không hợp lệ: {value}")
    };

    public bool CanTransitionTo(OrderStatus next)
    {
        // Định nghĩa state machine ngay trong VO
        return (this, next) switch
        {
            _ when this == Pending && next == Confirmed => true,
            _ when this == Pending && next == Cancelled => true,
            _ when this == Confirmed && next == Shipped => true,
            _ when this == Confirmed && next == Cancelled => true,
            _ when this == Shipped && next == Delivered => true,
            _ => false
        };
    }

    public override string ToString() => Value;
}

