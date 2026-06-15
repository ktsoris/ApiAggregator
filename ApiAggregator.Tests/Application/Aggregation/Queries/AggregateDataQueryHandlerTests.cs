using ApiAggregator.Application.Aggregation.Filtering;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Aggregation.Queries;
using ApiAggregator.Application.Aggregation.Sorting;
using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Application.Statistics.Store;
using ApiAggregator.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ApiAggregator.Tests.Application.Aggregation.Queries;

public sealed class AggregateDataQueryHandlerTests
{
    private static IExternalApiClient CreateClient(
        string sourceName,
        ExternalApiFetchResult result)
    {
        var client = Substitute.For<IExternalApiClient>();
        client.SourceName.Returns(sourceName);
        client.FetchAsync(Arg.Any<AggregateDataInput>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(result));
        return client;
    }

    private static AggregatedItemDto CreateItem(string source, string title)
    {
        return new AggregatedItemDto
        {
            Source = source,
            Title = title,
            Description = "Description",
            Category = "technology",
            PublishedAt = DateTimeOffset.UtcNow,
            RelevanceScore = 50,
            Url = "https://example.com"
        };
    }

    private static AggregateDataQueryHandler CreateHandler(
        IEnumerable<IExternalApiClient> clients,
        IApiRequestLogRepository? logRepository = null,
        IApiStatisticsStore? statisticsStore = null)
    {
        return new AggregateDataQueryHandler(
            clients,
            logRepository ?? Substitute.For<IApiRequestLogRepository>(),
            new AggregatedItemFilter(),
            new AggregatedItemSorter(),
            statisticsStore ?? new InMemoryApiStatisticsStore(),
            Substitute.For<ILogger<AggregateDataQueryHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldCallAllProviders_AndCombineResults()
    {
        var githubClient = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items = [CreateItem("GitHub", "Repo A")]
        });

        var hackerNewsClient = CreateClient("HackerNews", new ExternalApiFetchResult
        {
            Source = "HackerNews",
            Success = true,
            ResponseTimeMs = 60,
            Items = [CreateItem("HackerNews", "Story A")]
        });

        var handler = CreateHandler([githubClient, hackerNewsClient]);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput()),
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Select(i => i.Title).Should().Contain(["Repo A", "Story A"]);
        result.Providers.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldNotFail_WhenOneProviderFails()
    {
        var githubClient = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items = [CreateItem("GitHub", "Repo A")]
        });

        var failingClient = CreateClient("OpenMeteo", new ExternalApiFetchResult
        {
            Source = "OpenMeteo",
            Success = false,
            UsedFallback = false,
            ResponseTimeMs = 3000,
            ErrorMessage = "Provider unavailable.",
            Items = []
        });

        var handler = CreateHandler([githubClient, failingClient]);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput()),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Providers.Should().HaveCount(2);
        result.Providers.Single(p => p.Source == "OpenMeteo").Success.Should().BeFalse();
        result.Providers.Single(p => p.Source == "OpenMeteo").ErrorMessage.Should().Be("Provider unavailable.");
    }

    [Fact]
    public async Task Handle_ShouldIncludeProviderStatusMetadata()
    {
        var client = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            UsedFallback = false,
            ResponseTimeMs = 123,
            ErrorMessage = null,
            Items = []
        });

        var handler = CreateHandler([client]);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput()),
            CancellationToken.None);

        var provider = result.Providers.Single();
        provider.Source.Should().Be("GitHub");
        provider.Success.Should().BeTrue();
        provider.UsedFallback.Should().BeFalse();
        provider.ResponseTimeMs.Should().Be(123);
        provider.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldOnlyCallRequestedProviders_WhenProvidersFilterIsSet()
    {
        var githubClient = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items = [CreateItem("GitHub", "Repo A")]
        });

        var hackerNewsClient = CreateClient("HackerNews", new ExternalApiFetchResult
        {
            Source = "HackerNews",
            Success = true,
            ResponseTimeMs = 60,
            Items = [CreateItem("HackerNews", "Story A")]
        });

        var handler = CreateHandler([githubClient, hackerNewsClient]);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput { Providers = ["GitHub"] }),
            CancellationToken.None);

        result.Providers.Should().ContainSingle()
            .Which.Source.Should().Be("GitHub");

        await hackerNewsClient.DidNotReceive()
            .FetchAsync(Arg.Any<AggregateDataInput>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRecordStatistics_ForEachProvider()
    {
        var githubClient = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items = []
        });

        var failingClient = CreateClient("OpenMeteo", new ExternalApiFetchResult
        {
            Source = "OpenMeteo",
            Success = false,
            ResponseTimeMs = 250,
            ErrorMessage = "Timeout",
            Items = []
        });

        var statisticsStore = new InMemoryApiStatisticsStore();
        var handler = CreateHandler([githubClient, failingClient], statisticsStore: statisticsStore);

        await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput()),
            CancellationToken.None);

        var stats = statisticsStore.GetStatistics();

        stats.Should().Contain(s => s.ApiName == "GitHub" && s.SuccessfulRequests == 1);
        stats.Should().Contain(s => s.ApiName == "OpenMeteo" && s.FailedRequests == 1);
    }

    [Fact]
    public async Task Handle_ShouldStillReturnResult_WhenLogRepositoryThrows()
    {
        var client = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items = [CreateItem("GitHub", "Repo A")]
        });

        var failingRepository = Substitute.For<IApiRequestLogRepository>();
        failingRepository
            .When(x => x.AddAsync(Arg.Any<ApiRequestLog>(), Arg.Any<CancellationToken>()))
            .Do(_ => throw new InvalidOperationException("Database unavailable"));

        var handler = CreateHandler([client], logRepository: failingRepository);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput()),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Providers.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_ShouldApplyFilteringAndSorting()
    {
        var client = CreateClient("GitHub", new ExternalApiFetchResult
        {
            Source = "GitHub",
            Success = true,
            ResponseTimeMs = 50,
            Items =
            [
                CreateItem("GitHub", "Zebra Repo"),
                CreateItem("GitHub", "Apple Repo")
            ]
        });

        var handler = CreateHandler([client]);

        var result = await handler.Handle(
            new AggregateDataQuery(new AggregateDataInput { SortBy = "title" }),
            CancellationToken.None);

        result.Items.Select(i => i.Title).Should().ContainInOrder("Apple Repo", "Zebra Repo");
    }
}