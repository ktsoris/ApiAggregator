namespace ApiAggregator.Application.Statistics.Models;

public sealed record PerformanceBucketsDto
{
    public int Fast { get; init; }
    public int Average { get; init; }
    public int Slow { get; init; }
}