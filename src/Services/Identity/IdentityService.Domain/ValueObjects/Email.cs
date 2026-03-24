using IdentityService.Domain.Primitives;
using System.Text.RegularExpressions;

namespace IdentityService.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email không được rỗng");

        if (!EmailRegex.IsMatch(value))
            throw new DomainException($"Email '{value}' không hợp lệ");

        return new Email(value.ToLowerInvariant());
    }

    public override string ToString() => Value;
}
