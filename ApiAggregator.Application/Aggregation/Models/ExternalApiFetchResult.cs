namespace ApiAggregator.Application.Aggregation.Models;

public sealed record ExternalApiFetchResult
{
    public string Source { get; init; } = default!;
    public bool Success { get; init; }
    public bool UsedFallback { get; init; }
    public long ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
}