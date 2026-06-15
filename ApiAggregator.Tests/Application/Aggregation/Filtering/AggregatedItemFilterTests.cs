using ApiAggregator.Application.Aggregation.Filtering;
using ApiAggregator.Application.Aggregation.Models;
using FluentAssertions;
using Xunit;

namespace ApiAggregator.Tests.Application.Aggregation.Filtering;

public sealed class AggregatedItemFilterTests
{
    private readonly AggregatedItemFilter _sut = new();

    private static AggregatedItemDto CreateItem(
        string title = "Sample Title",
        string? description = "Sample Description",
        string? category = "technology",
        string source = "GitHub",
        DateTimeOffset? publishedAt = null)
    {
        return new AggregatedItemDto
        {
            Source = source,
            Title = title,
            Description = description,
            Category = category,
            PublishedAt = publishedAt ?? DateTimeOffset.UtcNow,
            RelevanceScore = 50,
            Url = "https://example.com"
        };
    }

    [Fact]
    public void Apply_ShouldFilterByKeyword_WhenKeywordMatchesTitle()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(title: "dotnet/runtime"),
            CreateItem(title: "react/react")
        };

        var input = new AggregateDataInput { Keyword = "dotnet" };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.Title.Should().Be("dotnet/runtime");
    }

    [Fact]
    public void Apply_ShouldFilterByKeyword_WhenKeywordMatchesDescription()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(title: "Item A", description: "Contains dotnet info"),
            CreateItem(title: "Item B", description: "Unrelated")
        };

        var input = new AggregateDataInput { Keyword = "dotnet" };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.Title.Should().Be("Item A");
    }

    [Fact]
    public void Apply_ShouldFilterByCategory()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(category: "technology"),
            CreateItem(category: "weather")
        };

        var input = new AggregateDataInput { Category = "weather" };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.Category.Should().Be("weather");
    }

    [Fact]
    public void Apply_ShouldFilterBySource()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(source: "GitHub"),
            CreateItem(source: "HackerNews")
        };

        var input = new AggregateDataInput { Source = "HackerNews" };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.Source.Should().Be("HackerNews");
    }

    [Fact]
    public void Apply_ShouldFilterByFromDate()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateItem(publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero))
        };

        var input = new AggregateDataInput
        {
            FromDate = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.PublishedAt.Should().Be(new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Apply_ShouldFilterByToDate()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(publishedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)),
            CreateItem(publishedAt: new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero))
        };

        var input = new AggregateDataInput
        {
            ToDate = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)
        };

        var result = _sut.Apply(items, input);

        result.Should().ContainSingle()
            .Which.PublishedAt.Should().Be(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Apply_ShouldReturnAllItems_WhenNoFiltersProvided()
    {
        var items = new List<AggregatedItemDto>
        {
            CreateItem(title: "Item A"),
            CreateItem(title: "Item B")
        };

        var input = new AggregateDataInput();

        var result = _sut.Apply(items, input);

        result.Should().HaveCount(2);
    }
}