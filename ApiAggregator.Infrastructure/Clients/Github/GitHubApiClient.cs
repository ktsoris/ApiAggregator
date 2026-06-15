using System.Diagnostics;
using System.Net.Http.Json;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Caching;
using ApiAggregator.Application.Common.Interfaces;

namespace ApiAggregator.Infrastructure.Clients.GitHub;

public sealed class GitHubApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IApiResponseCache _cache;
    private readonly IApiCacheKeyFactory _cacheKeyFactory;
    private readonly IAiErrorEnricher _aiErrorEnricher;

    public string SourceName => "GitHub";

    public GitHubApiClient(
        HttpClient httpClient,
        IApiResponseCache cache,
        IApiCacheKeyFactory cacheKeyFactory,
        IAiErrorEnricher aiErrorEnricher)
    {
        _httpClient = httpClient;
        _cache = cache;
        _cacheKeyFactory = cacheKeyFactory;
        _aiErrorEnricher = aiErrorEnricher;

        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ApiAggregator");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
    }

    public async Task<ExternalApiFetchResult> FetchAsync(
        AggregateDataInput input,
        CancellationToken cancellationToken)
    {
        var keyword = input.Keyword ?? "dotnet";
        var cacheKey = _cacheKeyFactory.CreateProviderCacheKey(SourceName, input);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetAsync(
                $"search/repositories?q={Uri.EscapeDataString(keyword)}&sort=stars&per_page=10",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var data = await response.Content
                .ReadFromJsonAsync<GitHubSearchResponse>(
                    cancellationToken: cancellationToken);

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

            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            var cached = await _cache.GetAsync<ExternalApiFetchResult>(cacheKey, cancellationToken);

            if (cached is not null)
            {
                return cached with
                {
                    UsedFallback = true,
                    Success = false,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ErrorMessage = $"Live fetch failed. Returned cached data. Reason: {ex.Message}"
                };
            }

            var friendlyError = await _aiErrorEnricher.EnrichAsync(
                SourceName,
                ex.Message,
                cancellationToken);

            return new ExternalApiFetchResult
            {
                Source = SourceName,
                Success = false,
                UsedFallback = false,
                ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                ErrorMessage = friendlyError
            };
        }
    }
}