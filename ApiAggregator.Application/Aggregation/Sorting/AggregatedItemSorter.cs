using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Aggregation.Sorting;

public sealed class AggregatedItemSorter : IAggregatedItemSorter
{
    private static readonly HashSet<string> ValidSortFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "date", "relevance", "category", "source", "title"
    };

    public IReadOnlyList<AggregatedItemDto> Apply(
        IReadOnlyList<AggregatedItemDto> items,
        AggregateDataInput input)
    {
        if (string.IsNullOrWhiteSpace(input.SortBy))
        {
            return items;
        }

        if (!ValidSortFields.Contains(input.SortBy))
        {
            throw new InvalidSortFieldException(input.SortBy);
        }

        var descending = string.Equals(input.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedEnumerable<AggregatedItemDto> ordered = input.SortBy.ToLowerInvariant() switch
        {
            "date" => descending
                ? items.OrderByDescending(i => i.PublishedAt)
                : items.OrderBy(i => i.PublishedAt),

            "relevance" => descending
                ? items.OrderByDescending(i => i.RelevanceScore)
                : items.OrderBy(i => i.RelevanceScore),

            "category" => descending
                ? items.OrderByDescending(i => i.Category, StringComparer.OrdinalIgnoreCase)
                : items.OrderBy(i => i.Category, StringComparer.OrdinalIgnoreCase),

            "source" => descending
                ? items.OrderByDescending(i => i.Source, StringComparer.OrdinalIgnoreCase)
                : items.OrderBy(i => i.Source, StringComparer.OrdinalIgnoreCase),

            "title" => descending
                ? items.OrderByDescending(i => i.Title, StringComparer.OrdinalIgnoreCase)
                : items.OrderBy(i => i.Title, StringComparer.OrdinalIgnoreCase),

            _ => throw new InvalidSortFieldException(input.SortBy)
        };

        return ordered.ToList();
    }
}