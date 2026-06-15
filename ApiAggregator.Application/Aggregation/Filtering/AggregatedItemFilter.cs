using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Aggregation.Filtering;

public sealed class AggregatedItemFilter : IAggregatedItemFilter
{
    public IReadOnlyList<AggregatedItemDto> Apply(
        IReadOnlyList<AggregatedItemDto> items,
        AggregateDataInput input)
    {
        var result = items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            result = result.Where(i =>
                (i.Title?.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (i.Description?.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            result = result.Where(i =>
                string.Equals(i.Category, input.Category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(input.Source))
        {
            result = result.Where(i =>
                string.Equals(i.Source, input.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (input.FromDate.HasValue)
        {
            result = result.Where(i =>
                i.PublishedAt.HasValue && i.PublishedAt.Value >= input.FromDate.Value);
        }

        if (input.ToDate.HasValue)
        {
            result = result.Where(i =>
                i.PublishedAt.HasValue && i.PublishedAt.Value <= input.ToDate.Value);
        }

        return result.ToList();
    }
}