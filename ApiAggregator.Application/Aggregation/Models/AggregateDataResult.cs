namespace ApiAggregator.Application.Aggregation.Models;

public sealed record AggregateDataResult
{
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
    public IReadOnlyList<ProviderResultDto> Providers { get; init; } = [];
}