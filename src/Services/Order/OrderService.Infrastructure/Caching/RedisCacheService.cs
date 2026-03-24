using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using StackExchange.Redis;

namespace OrderService.Infrastructure.Caching;

internal sealed class RedisCacheService(
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    // Default 5 phút
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    public async Task<T?> GetAsync<T>(
        string key, CancellationToken ct = default)
        where T : class
    {
        try
        {
            var cached = await cache.GetStringAsync(key, ct);

            if (cached is null)
            {
                logger.LogDebug("Cache MISS: {Key}", key);
                return null;
            }

            logger.LogDebug("Cache HIT: {Key}", key);
            return JsonSerializer.Deserialize<T>(cached);
        }
        catch (Exception ex)
        {
            // Cache fail không được làm app fail
            // → log lỗi và trả null để fallback về DB
            logger.LogError(ex, "Redis GET failed for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(
        string key, T value,
        TimeSpan? expiry = null,
        CancellationToken ct = default)
        where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
            };

            var json = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, json, options, ct);

            logger.LogDebug("Cache SET: {Key} (expiry: {Expiry})",
                key, expiry ?? DefaultExpiry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis SET failed for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(
        string key, CancellationToken ct = default)
    {
        try
        {
            await cache.RemoveAsync(key, ct);
            logger.LogDebug("Cache REMOVE: {Key}", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redis REMOVE failed for key: {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(
        string prefix, CancellationToken ct = default)
    {
        try
        {
            // Xoá tất cả keys có prefix — dùng khi invalidate cache
            var server = redis.GetServer(
                redis.GetEndPoints().First());

            var keys = server.Keys(pattern: $"{prefix}*")
                .Select(k => (string)k!)
                .ToArray();

            foreach (var key in keys)
                await cache.RemoveAsync(key, ct);

            logger.LogDebug(
                "Cache REMOVE by prefix: {Prefix} ({Count} keys)",
                prefix, keys.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Redis REMOVE by prefix failed: {Prefix}", prefix);
        }
    }
}