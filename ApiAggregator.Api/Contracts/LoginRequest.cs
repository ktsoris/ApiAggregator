namespace ApiAggregator.Api.Contracts;

public sealed record LoginRequest
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}