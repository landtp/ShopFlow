using IdentityService.Application.Abstractions;

namespace IdentityService.Infrastructure.Services;

internal sealed class BCryptPasswordHasher : IPasswordHasher
{
    // WorkFactor 12 = ~300ms per hash trên modern hardware
    // Đủ chậm để brute-force tốn kém, đủ nhanh để UX tốt
    private const int WorkFactor = 12;

    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
