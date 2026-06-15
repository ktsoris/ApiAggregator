using System.Text.Json;
using ApiAggregator.Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace ApiAggregator.Infrastructure.Caching;

/// <summary>
/// Redis-backed implementation of <see cref="IApiResponseCache"/> using
/// <see cref="IDistributedCache"/>. To activate this implementation:
///
/// 1. Install the NuGet package: Microsoft.Extensions.Caching.StackExchangeRedis
/// 2. Register Redis in DependencyInjection.cs:
///
///    services.AddStackExchangeRedisCache(options =>
///    {
///        options.Configuration = configuration.GetConnectionString("Redis");
///    });
///
/// 3. Replace the registration of IApiResponseCache from
///    MemoryApiResponseCache to RedisApiResponseCache.
///
/// No other code needs to change — the aggregation handler and providers
/// depend only on IApiResponseCache.
/// </summary>
public sealed class RedisApiResponseCache : IApiResponseCache
{
    private readonly IDistributedCache _distributedCache;

    public RedisApiResponseCache(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken) where T : class
    {
        var bytes = await _distributedCache.GetAsync(cacheKey, cancellationToken);

        if (bytes is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(bytes);
    }

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan ttl, CancellationToken cancellationToken) where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        await _distributedCache.SetAsync(cacheKey, bytes, options, cancellationToken);
    }
}