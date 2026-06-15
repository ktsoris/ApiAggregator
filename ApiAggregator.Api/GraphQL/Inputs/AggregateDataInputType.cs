namespace ApiAggregator.Api.GraphQL.Inputs;

public sealed record AggregateDataInputType
{
    public string? Keyword { get; init; }
    public string? Category { get; init; }
    public string? Source { get; init; }
    public DateTimeOffset? FromDate { get; init; }
    public DateTimeOffset? ToDate { get; init; }
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
    public List<string>? Providers { get; init; }
}