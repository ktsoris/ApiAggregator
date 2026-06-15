using ApiAggregator.Application.Statistics.Models;

namespace ApiAggregator.Application.Statistics.Store;

public interface IApiStatisticsStore
{
    void RecordSuccess(string apiName, long responseTimeMs);
    void RecordFailure(string apiName, long responseTimeMs);
    IReadOnlyList<ApiStatisticsDto> GetStatistics();
}