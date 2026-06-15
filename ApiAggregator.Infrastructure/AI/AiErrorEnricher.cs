using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Infrastructure.AI;

public sealed class AiErrorEnricher : IAiErrorEnricher
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<AiErrorEnricher> _logger;

    public AiErrorEnricher(
        HttpClient httpClient,
        IOptions<OpenAiSettings> settings,
        ILogger<AiErrorEnricher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.openai.com/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<string> EnrichAsync(
        string providerName,
        string rawError,
        CancellationToken cancellationToken)
    {
        try
        {
            var prompt = $"""
                A provider named '{providerName}' failed with the following technical error:
                '{rawError}'

                Write a single, short, user-friendly error message (max 2 sentences) that:
                - Explains what went wrong in plain English
                - Does not expose internal technical details
                - Is professional and calm in tone

                Respond with only the error message, nothing else.
                """;

            var request = new OpenAiRequest
            {
                Model = _settings.Model,
                Messages =
                [
                    new OpenAiMessage { Role = "user", Content = prompt }
                ],
                MaxTokens = 150
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/chat/completions",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<OpenAiResponse>(
                    cancellationToken: cancellationToken);

            return result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim()
                ?? $"The {providerName} provider is currently unavailable.";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "AI error enrichment failed for provider {Provider}. Falling back to raw error.",
                providerName);

            return $"The {providerName} provider is currently unavailable.";
        }
    }
}

public sealed class OpenAiRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = default!;

    [JsonPropertyName("messages")]
    public List<OpenAiMessage> Messages { get; init; } = [];

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; init; }
}

public sealed class OpenAiMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = default!;

    [JsonPropertyName("content")]
    public string Content { get; init; } = default!;
}

public sealed class OpenAiResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAiChoice>? Choices { get; init; }
}

public sealed class OpenAiChoice
{
    [JsonPropertyName("message")]
    public OpenAiMessage? Message { get; init; }
}