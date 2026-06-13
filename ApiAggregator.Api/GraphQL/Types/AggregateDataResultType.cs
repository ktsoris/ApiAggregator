namespace ApiAggregator.Api.GraphQL.Types;

public sealed record AggregateDataResultType
{
    public List<AggregatedItemType> Items { get; init; } = [];
    public List<ProviderStatusType> Providers { get; init; } = [];
}