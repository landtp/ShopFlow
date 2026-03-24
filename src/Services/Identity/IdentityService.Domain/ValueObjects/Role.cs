using IdentityService.Domain.Primitives;

namespace IdentityService.Domain.ValueObjects;

public sealed record Role
{
    public static readonly Role Customer = new("Customer");
    public static readonly Role Admin = new("Admin");

    public string Value { get; }

    private Role(string value) => Value = value;

    public static Role From(string value) => value switch
    {
        "Customer" => Customer,
        "Admin" => Admin,
        _ => throw new DomainException($"Role không hợp lệ: {value}")
    };

    public override string ToString() => Value;
}
