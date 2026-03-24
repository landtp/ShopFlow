using IdentityService.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace IdentityService.Infrastructure.Services;

internal sealed class RedisTokenBlacklistService(
    IDistributedCache cache,
    ILogger<RedisTokenBlacklistService> logger)
    : ITokenBlacklistService
{
    public async Task BlacklistAsync(
        string token, TimeSpan expiry,
        CancellationToken ct = default)
    {
        var key = $"blacklist:{token}";

        await cache.SetStringAsync(key, "revoked",
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            }, ct);

        logger.LogInformation("Token blacklisted, expires in {Expiry}", expiry);
    }

    public async Task<bool> IsBlacklistedAsync(
        string token, CancellationToken ct = default)
    {
        var key = $"blacklist:{token}";
        var value = await cache.GetStringAsync(key, ct);
        return value is not null;
    }
}