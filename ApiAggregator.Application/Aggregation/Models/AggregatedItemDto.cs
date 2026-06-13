namespace ApiAggregator.Application.Aggregation.Models;

public sealed record AggregatedItemDto
{
    public string Source { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public double? RelevanceScore { get; init; }
    public string? Url { get; init; }
}