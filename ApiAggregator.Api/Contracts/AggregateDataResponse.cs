namespace ApiAggregator.Api.Contracts;

public sealed record AggregatedItemResponse
{
    public string Source { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public double? RelevanceScore { get; init; }
    public string? Url { get; init; }
}

public sealed record ProviderStatusResponse
{
    public string Source { get; init; } = default!;
    public bool Success { get; init; }
    public bool UsedFallback { get; init; }
    public long ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
}

public sealed record AggregateDataResponse
{
    public List<AggregatedItemResponse> Items { get; init; } = [];
    public List<ProviderStatusResponse> Providers { get; init; } = [];
}