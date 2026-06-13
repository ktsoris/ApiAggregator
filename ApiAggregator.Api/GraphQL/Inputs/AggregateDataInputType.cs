namespace ApiAggregator.Api.GraphQL.Inputs;

public sealed record AggregateDataInputType
{
    public string? Keyword { get; init; }
    public string? Category { get; init; }
    public string? SortBy { get; init; }
    public List<string>? Providers { get; init; }
}