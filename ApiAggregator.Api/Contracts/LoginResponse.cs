namespace ApiAggregator.Api.Contracts;

public sealed record LoginResponse
{
    public bool Success { get; init; }
    public string? AccessToken { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}