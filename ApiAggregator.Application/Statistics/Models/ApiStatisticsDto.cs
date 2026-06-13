namespace ApiAggregator.Application.Statistics.Models;

public sealed record ApiStatisticsDto
{
    public string ApiName { get; init; } = default!;
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public PerformanceBucketsDto Buckets { get; init; } = default!;
}