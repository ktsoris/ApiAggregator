namespace ApiAggregator.Api.GraphQL.Types;

public sealed record ProviderStatusType
{
    public string Source { get; init; } = default!;
    public bool Success { get; init; }
    public bool UsedFallback { get; init; }
    public long ResponseTimeMs { get; init; }
    public string? ErrorMessage { get; init; }
}