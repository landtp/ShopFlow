namespace IdentityService.Application.Abstractions;

public interface ITokenBlacklistService
{
    Task BlacklistAsync(string token, TimeSpan expiry, CancellationToken ct = default);
    Task<bool> IsBlacklistedAsync(string token, CancellationToken ct = default);
}
