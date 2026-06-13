using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Infrastructure.Clients.OpenMeteo;

public sealed class OpenMeteoApiClient : IExternalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    public string SourceName => "OpenMeteo";

    public OpenMeteoApiClient(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
        _cache = cache;
    }

    public async Task<ExternalApiFetchResult> FetchAsync(
        AggregateDataInput input,
        CancellationToken cancellationToken)
    {
        var cacheKey = "openmeteo:athens";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await _httpClient.GetFromJsonAsync<OpenMeteoResponse>(
                "forecast?latitude=37.9795&longitude=23.7162&current=temperature_2m,weathercode,windspeed_10m&timezone=auto",
                cancellationToken);

            stopwatch.Stop();

            var items = new List<AggregatedItemDto>();

            if (response?.Current is not null)
            {
                items.Add(new AggregatedItemDto
                {
                    Source = SourceName,
                    Title = $"Athens Weather — {response.Current.Temperature}°C",
                    Description = $"Wind: {response.Current.WindSpeed} km/h | Code: {response.Current.WeatherCode}",
                    Category = "weather",
                    PublishedAt = DateTimeOffset.UtcNow,
                    RelevanceScore = 50,
                    Url = "https://open-meteo.com"
                });
            }

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

public sealed class OpenMeteoResponse
{
    [JsonPropertyName("current")]
    public OpenMeteoCurrent? Current { get; init; }
}

public sealed class OpenMeteoCurrent
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; init; }

    [JsonPropertyName("weathercode")]
    public int WeatherCode { get; init; }

    [JsonPropertyName("windspeed_10m")]
    public double WindSpeed { get; init; }
}