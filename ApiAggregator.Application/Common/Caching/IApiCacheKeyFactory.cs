using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Common.Caching;

public interface IApiCacheKeyFactory
{
    string CreateProviderCacheKey(string providerName, AggregateDataInput input);
}