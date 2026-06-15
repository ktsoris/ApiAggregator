namespace ApiAggregator.Application.Common.Caching;

public interface IApiResponseCache
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken) where T : class;

    Task SetAsync<T>(string cacheKey, T value, TimeSpan ttl, CancellationToken cancellationToken) where T : class;
}