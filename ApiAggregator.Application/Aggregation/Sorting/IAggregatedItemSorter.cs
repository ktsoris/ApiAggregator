using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Aggregation.Sorting;

public interface IAggregatedItemSorter
{
    IReadOnlyList<AggregatedItemDto> Apply(
        IReadOnlyList<AggregatedItemDto> items,
        AggregateDataInput input);
}