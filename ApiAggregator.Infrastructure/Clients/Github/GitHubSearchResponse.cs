using System.Text.Json.Serialization;

namespace ApiAggregator.Infrastructure.Clients.GitHub;

public sealed class GitHubSearchResponse
{
    [JsonPropertyName("items")]
    public List<GitHubRepository> Items { get; init; } = [];
}

public sealed class GitHubRepository
{
    [JsonPropertyName("full_name")]
    public string FullName { get; init; } = default!;

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; init; } = default!;

    [JsonPropertyName("stargazers_count")]
    public int StargazersCount { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    [JsonPropertyName("topics")]
    public List<string> Topics { get; init; } = [];
}