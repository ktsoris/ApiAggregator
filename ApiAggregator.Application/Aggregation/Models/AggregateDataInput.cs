namespace ApiAggregator.Application.Aggregation.Models;

public sealed record AggregateDataInput
{
    public string? Keyword { get; init; }
    public string? Category { get; init; }
    public string? SortBy { get; init; }
    public IReadOnlyList<string>? Providers { get; init; }
}