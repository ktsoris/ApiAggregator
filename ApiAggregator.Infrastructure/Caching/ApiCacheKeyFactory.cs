using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Caching;

namespace ApiAggregator.Infrastructure.Caching;

public sealed class ApiCacheKeyFactory : IApiCacheKeyFactory
{
    public string CreateProviderCacheKey(string providerName, AggregateDataInput input)
    {
        var keyword = input.Keyword ?? "null";
        var category = input.Category ?? "null";
        var from = input.FromDate?.ToUnixTimeSeconds().ToString() ?? "null";
        var to = input.ToDate?.ToUnixTimeSeconds().ToString() ?? "null";

        return $"api-aggregator:provider:{providerName.ToLowerInvariant()}:keyword={keyword}:category={category}:from={from}:to={to}";
    }
}