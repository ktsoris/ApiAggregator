using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Application.Statistics.Models;
using MediatR;

namespace ApiAggregator.Application.Statistics.Queries;

public sealed class GetApiStatisticsQueryHandler
    : IRequestHandler<GetApiStatisticsQuery, IReadOnlyList<ApiStatisticsDto>>
{
    private readonly IApiRequestLogRepository _repository;

    public GetApiStatisticsQueryHandler(IApiRequestLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ApiStatisticsDto>> Handle(
        GetApiStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var logs = await _repository.GetRecentAsync(
            TimeSpan.FromHours(24),
            cancellationToken);

        return logs
            .GroupBy(l => l.ApiName)
            .Select(g =>
            {
                var total = g.Count();
                var successful = g.Count(l => l.Success);
                var failed = total - successful;
                var avgMs = g.Average(l => l.ResponseTimeMs);

                var fast = g.Count(l => l.ResponseTimeMs < 100);
                var average = g.Count(l => l.ResponseTimeMs >= 100 && l.ResponseTimeMs <= 300);
                var slow = g.Count(l => l.ResponseTimeMs > 300);

                return new ApiStatisticsDto
                {
                    ApiName = g.Key,
                    TotalRequests = total,
                    SuccessfulRequests = successful,
                    FailedRequests = failed,
                    AverageResponseTimeMs = Math.Round(avgMs, 2),
                    Buckets = new PerformanceBucketsDto
                    {
                        Fast = fast,
                        Average = average,
                        Slow = slow
                    }
                };
            })
            .ToList();
    }
}