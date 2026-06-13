namespace ApiAggregator.Api.GraphQL.Types;

public sealed record LoginResultType
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
}