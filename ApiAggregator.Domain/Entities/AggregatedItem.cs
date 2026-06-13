namespace ApiAggregator.Domain.Entities;

public sealed class AggregatedItem
{
    public Guid Id { get; private set; }
    public string Source { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public string? Category { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public double? RelevanceScore { get; private set; }
    public string? Url { get; private set; }
    public DateTimeOffset RetrievedAt { get; private set; }

    private AggregatedItem() { }

    public static AggregatedItem Create(
        string source,
        string title,
        string? description,
        string? category,
        DateTimeOffset? publishedAt,
        double? relevanceScore,
        string? url)
    {
        return new AggregatedItem
        {
            Id = Guid.NewGuid(),
            Source = source,
            Title = title,
            Description = description,
            Category = category,
            PublishedAt = publishedAt,
            RelevanceScore = relevanceScore,
            Url = url,
            RetrievedAt = DateTimeOffset.UtcNow
        };
    }
}