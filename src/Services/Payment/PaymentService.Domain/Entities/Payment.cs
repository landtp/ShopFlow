namespace PaymentService.Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? FailReason { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid customerId,
        decimal amount,
        string currency)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Currency = currency,
            Status = "Processing",
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkSuccess() => Status = "Completed";

    public void MarkFailed(string reason)
    {
        Status = "Failed";
        FailReason = reason;
    }
}