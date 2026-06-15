using ApiAggregator.Application.Statistics.Store;
using FluentAssertions;
using Xunit;

namespace ApiAggregator.Tests.Application.Statistics.Store;

public sealed class InMemoryApiStatisticsStoreTests
{
    [Fact]
    public void RecordSuccess_ShouldIncrementTotalAndSuccessfulRequests()
    {
        var store = new InMemoryApiStatisticsStore();

        store.RecordSuccess("GitHub", 50);

        var stats = store.GetStatistics().Single(s => s.ApiName == "GitHub");

        stats.TotalRequests.Should().Be(1);
        stats.SuccessfulRequests.Should().Be(1);
        stats.FailedRequests.Should().Be(0);
    }

    [Fact]
    public void RecordFailure_ShouldIncrementTotalAndFailedRequests()
    {
        var store = new InMemoryApiStatisticsStore();

        store.RecordFailure("GitHub", 500);

        var stats = store.GetStatistics().Single(s => s.ApiName == "GitHub");

        stats.TotalRequests.Should().Be(1);
        stats.SuccessfulRequests.Should().Be(0);
        stats.FailedRequests.Should().Be(1);
    }

    [Fact]
    public void GetStatistics_ShouldCalculateAverageResponseTime()
    {
        var store = new InMemoryApiStatisticsStore();

        store.RecordSuccess("GitHub", 100);
        store.RecordSuccess("GitHub", 200);

        var stats = store.GetStatistics().Single(s => s.ApiName == "GitHub");

        stats.AverageResponseTimeMs.Should().Be(150);
    }

    [Theory]
    [InlineData(50, 1, 0, 0)]   // fast: < 100
    [InlineData(150, 0, 1, 0)]  // average: 100-200
    [InlineData(250, 0, 0, 1)]  // slow: > 200
    public void GetStatistics_ShouldPlaceRequestInCorrectBucket(
        long responseTimeMs, int expectedFast, int expectedAverage, int expectedSlow)
    {
        var store = new InMemoryApiStatisticsStore();

        store.RecordSuccess("GitHub", responseTimeMs);

        var stats = store.GetStatistics().Single(s => s.ApiName == "GitHub");

        stats.Buckets.Fast.Should().Be(expectedFast);
        stats.Buckets.Average.Should().Be(expectedAverage);
        stats.Buckets.Slow.Should().Be(expectedSlow);
    }

    [Fact]
    public void GetStatistics_ShouldHandleConcurrentWritesSafely()
    {
        var store = new InMemoryApiStatisticsStore();
        const int iterations = 1000;

        Parallel.For(0, iterations, i =>
        {
            if (i % 2 == 0)
            {
                store.RecordSuccess("GitHub", 50);
            }
            else
            {
                store.RecordFailure("GitHub", 250);
            }
        });

        var stats = store.GetStatistics().Single(s => s.ApiName == "GitHub");

        stats.TotalRequests.Should().Be(iterations);
        stats.SuccessfulRequests.Should().Be(iterations / 2);
        stats.FailedRequests.Should().Be(iterations / 2);
    }

    [Fact]
    public void GetStatistics_ShouldTrackMultipleApisIndependently()
    {
        var store = new InMemoryApiStatisticsStore();

        store.RecordSuccess("GitHub", 50);
        store.RecordSuccess("HackerNews", 150);

        var stats = store.GetStatistics();

        stats.Should().HaveCount(2);
        stats.Should().Contain(s => s.ApiName == "GitHub" && s.TotalRequests == 1);
        stats.Should().Contain(s => s.ApiName == "HackerNews" && s.TotalRequests == 1);
    }
}