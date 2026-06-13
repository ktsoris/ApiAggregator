using System.Diagnostics;
using System.Net.Http.Json;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Infrastructure.Clients.GitHub;

public sealed class GitHubApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public string SourceName => "GitHub";

    public GitHubApiClient(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ApiAggregator");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        _cache = cache;
    }

    public async Task<ExternalApiFetchResult> FetchAsync(
        AggregateDataInput input,
        CancellationToken cancellationToken)
    {
        var keyword = input.Keyword ?? "dotnet";
        var cacheKey = $"github:{keyword}";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(
                $"search/repositories?q={Uri.EscapeDataString(keyword)}&sort=stars&per_page=10",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var data = await response.Content
                .ReadFromJsonAsync<GitHubSearchResponse>(cancellationToken: cancellationToken);

            stopwatch.Stop();

            var items = data?.Items.Select(r => new AggregatedItemDto
            {
                Source = SourceName,
                Title = r.FullName,
                Description = r.Description,
                Category = r.Topics.FirstOrDefault() ?? "repository",
                PublishedAt = r.CreatedAt,
                RelevanceScore = Math.Min(r.StargazersCount / 1000.0 * 10, 100),
                Url = r.HtmlUrl
            }).ToList() ?? [];

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