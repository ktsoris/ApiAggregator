using System.Collections.Concurrent;
using ApiAggregator.Application.Statistics.Models;

namespace ApiAggregator.Application.Statistics.Store;

public sealed class InMemoryApiStatisticsStore : IApiStatisticsStore
{
    private readonly ConcurrentDictionary<string, ApiStatisticsAccumulator> _accumulators = new();

    public void RecordSuccess(string apiName, long responseTimeMs)
    {
        Record(apiName, success: true, responseTimeMs);
    }

    public void RecordFailure(string apiName, long responseTimeMs)
    {
        Record(apiName, success: false, responseTimeMs);
    }

    private void Record(string apiName, bool success, long responseTimeMs)
    {
        var accumulator = _accumulators.GetOrAdd(apiName, _ => new ApiStatisticsAccumulator());
        accumulator.Record(success, responseTimeMs);
    }

    public IReadOnlyList<ApiStatisticsDto> GetStatistics()
    {
        return _accumulators
            .Select(kvp => new ApiStatisticsDto
            {
                ApiName = kvp.Key,
                TotalRequests = (int)kvp.Value.TotalRequests,
                SuccessfulRequests = (int)kvp.Value.SuccessfulRequests,
                FailedRequests = (int)kvp.Value.FailedRequests,
                AverageResponseTimeMs = kvp.Value.AverageResponseTimeMs,
                Buckets = new PerformanceBucketsDto
                {
                    Fast = (int)kvp.Value.FastCount,
                    Average = (int)kvp.Value.AverageCount,
                    Slow = (int)kvp.Value.SlowCount
                }
            })
            .ToList();
    }
}