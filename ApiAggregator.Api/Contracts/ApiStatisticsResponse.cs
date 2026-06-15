namespace ApiAggregator.Api.Contracts;

public sealed record PerformanceBucketsResponse
{
    public int Fast { get; init; }
    public int Average { get; init; }
    public int Slow { get; init; }
}

public sealed record ApiStatisticsResponse
{
    public string ApiName { get; init; } = default!;
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public PerformanceBucketsResponse Buckets { get; init; } = default!;
}

public sealed record ApiStatisticsListResponse
{
    public List<ApiStatisticsResponse> Apis { get; init; } = [];
}