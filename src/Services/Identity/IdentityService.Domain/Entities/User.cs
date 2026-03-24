using IdentityService.Domain.Primitives;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Entities;

public sealed class User : Entity
{
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Role Role { get; private set; } = null!;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // EF Core constructor
    private User() { }

    // Factory method — cửa vào duy nhất để tạo User
    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        Role? role = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("Tên không được rỗng");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Họ không được rỗng");

        return new User
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role ?? Role.Customer,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // ── Behaviors ──────────────────────────────────────────────

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
    }

    public bool IsRefreshTokenValid(string token) =>
        RefreshToken == token &&
        RefreshTokenExpiresAt > DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}
