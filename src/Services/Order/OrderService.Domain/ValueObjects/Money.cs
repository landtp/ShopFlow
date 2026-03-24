using OrderService.Domain.Primitives;

namespace OrderService.Domain.ValueObjects;

// record tự implement Equals + GetHashCode dựa trên properties → đúng VO semantics
public sealed record Money(decimal Amount, string Currency)
{
    public static readonly Money Zero = new(0, "VND");

    // Validation nằm trong VO, không để handler tự check
    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Số tiền không được âm");
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency không được rỗng");

        return new(amount, currency.ToUpperInvariant());
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new DomainException($"Không thể cộng {a.Currency} với {b.Currency}");
        return new(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator *(Money money, int multiplier) =>
        new(money.Amount * multiplier, money.Currency);

    public override string ToString() =>
        $"{Amount:N0} {Currency}";
}

