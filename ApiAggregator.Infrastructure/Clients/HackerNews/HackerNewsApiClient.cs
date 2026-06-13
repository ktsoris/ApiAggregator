using System.Diagnostics;
using System.Net.Http.Json;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Infrastructure.Clients.HackerNews;

public sealed class HackerNewsApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public string SourceName => "HackerNews";

    public HackerNewsApiClient(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
        _cache = cache;
    }

    public async Task<ExternalApiFetchResult> FetchAsync(
        AggregateDataInput input,
        CancellationToken cancellationToken)
    {
        var cacheKey = "hackernews:topstories";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var ids = await _httpClient.GetFromJsonAsync<List<int>>(
                "topstories.json",
                cancellationToken);

            var top10 = ids?.Take(10).ToList() ?? [];

            var storyTasks = top10.Select(id =>
                _httpClient.GetFromJsonAsync<HackerNewsStory>(
                    $"item/{id}.json",
                    cancellationToken));

            var stories = await Task.WhenAll(storyTasks);

            stopwatch.Stop();

            var keyword = input.Keyword?.ToLowerInvariant();

            var items = stories
                .Where(s => s is not null)
                .Where(s => string.IsNullOrWhiteSpace(keyword) ||
                            (s!.Title?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .Select(s => new AggregatedItemDto
                {
                    Source = SourceName,
                    Title = s!.Title ?? "Untitled",
                    Description = $"Score: {s.Score} | By: {s.By}",
                    Category = "news",
                    PublishedAt = s.Time.HasValue
                        ? DateTimeOffset.FromUnixTimeSeconds(s.Time.Value)
                        : null,
                    RelevanceScore = Math.Min((s.Score ?? 0) / 10.0, 100),
                    Url = s.Url
                }).ToList();

            var result = new ExternalApiFetchResult
            {
                Source = SourceName,
                Success = true,
                UsedFallback = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                Items = items
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            if (_cache.TryGetValue(cacheKey, out ExternalApiFetchResult? cached) && cached is not null)
            {
                return cached with
                {
                    UsedFallback = true,
                    Success = false,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = $"Live fetch failed. Returned cached data. Reason: {ex.Message}"
                };
            }

            return new ExternalApiFetchResult
            {
                Source = SourceName,
                Success = false,
                UsedFallback = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = ex.Message
            };
        }
    }
}

public sealed class HackerNewsStory
{
    public string? Title { get; init; }
    public string? Url { get; init; }
    public string? By { get; init; }
    public int? Score { get; init; }
    public long? Time { get; init; }
}