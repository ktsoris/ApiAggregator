using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Aggregation.Sorting;
using FluentAssertions;
using Xunit;

namespace ApiAggregator.Tests.Application.Aggregation.Sorting;

public sealed class AggregatedItemSorterTests
{
    private readonly AggregatedItemSorter _sut = new();

    private static AggregatedItemDto CreateItem(
        string title,
        string source = "GitHub",
        string? category = "technology",
        double? relevanceScore = 50,
        DateTimeOffset? publishedAt = null)
    {
        return new AggregatedItemDto
        {
            Source = source,
            Title = title,
            Description = "Description",
            Category = category,
            PublishedAt = publishedAt,
            RelevanceScore = relevanceScore,
            Url = "https://example.com"
        };
    }

    [Fact]
    public void Apply_ShouldReturnItemsUnchanged_WhenSortByIsNull()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("B"),
            CreateItem("A")
        };

        var input = new AggregateDataInput();

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("B", "A");
    }

    [Fact]
    public void Apply_ShouldSortByDateAscending_ByDefault()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Newer", publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateItem("Older", publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero))
        };

        var input = new AggregateDataInput { SortBy = "date" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("Older", "Newer");
    }

    [Fact]
    public void Apply_ShouldSortByDateDescending_WhenSortDirectionIsDesc()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Older", publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateItem("Newer", publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero))
        };

        var input = new AggregateDataInput { SortBy = "date", SortDirection = "desc" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("Newer", "Older");
    }

    [Fact]
    public void Apply_ShouldSortByRelevanceDescending()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Low", relevanceScore: 10),
            CreateItem("High", relevanceScore: 90)
        };

        var input = new AggregateDataInput { SortBy = "relevance", SortDirection = "desc" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("High", "Low");
    }

    [Fact]
    public void Apply_ShouldSortByCategory()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Weather Item", category: "weather"),
            CreateItem("News Item", category: "news")
        };

        var input = new AggregateDataInput { SortBy = "category" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("News Item", "Weather Item");
    }

    [Fact]
    public void Apply_ShouldSortBySource()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Item A", source: "HackerNews"),
            CreateItem("Item B", source: "GitHub")
        };

        var input = new AggregateDataInput { SortBy = "source" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Source).Should().ContainInOrder("GitHub", "HackerNews");
    }

    [Fact]
    public void Apply_ShouldSortByTitle()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem("Zebra"),
            CreateItem("Apple")
        };

        var input = new AggregateDataInput { SortBy = "title" };

        var result = _sut.Apply(items, input);

        result.Select(i => i.Title).Should().ContainInOrder("Apple", "Zebra");
    }

    [Fact]
    public void Apply_ShouldThrowInvalidSortFieldException_WhenSortByIsInvalid()
    {
        var items = new List<AggregatedItemDto> { CreateItem("Item A") };

        var input = new AggregateDataInput { SortBy = "invalid-field" };

        var act = () => _sut.Apply(items, input);

        act.Should().Throw<InvalidSortFieldException>()
            .WithMessage("*invalid-field*");
    }
}