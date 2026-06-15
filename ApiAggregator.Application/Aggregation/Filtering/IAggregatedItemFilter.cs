using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Aggregation.Filtering;

public interface IAggregatedItemFilter
{
    IReadOnlyList<AggregatedItemDto> Apply(
        IReadOnlyList<AggregatedItemDto> items,
        AggregateDataInput input);
}