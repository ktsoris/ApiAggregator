using ApiAggregator.Application.Common.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Infrastructure.Caching;

public sealed class MemoryApiResponseCache : IApiResponseCache
{
    private readonly IMemoryCache _cache;

    public MemoryApiResponseCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken) where T : class
    {
        _cache.TryGetValue(cacheKey, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string cacheKey, T value, TimeSpan ttl, CancellationToken cancellationToken) where T : class
    {
        _cache.Set(cacheKey, value, ttl);
        return Task.CompletedTask;
    }
}