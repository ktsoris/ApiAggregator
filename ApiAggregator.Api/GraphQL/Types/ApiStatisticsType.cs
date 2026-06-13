namespace ApiAggregator.Api.GraphQL.Types;

public sealed record ApiStatisticsType
{
    public string ApiName { get; init; } = default!;
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public PerformanceBucketsType Buckets { get; init; } = default!;
}

public sealed record PerformanceBucketsType
{
    public int Fast { get; init; }
    public int Average { get; init; }
    public int Slow { get; init; }
}