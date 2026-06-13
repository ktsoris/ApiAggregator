namespace ApiAggregator.Api.GraphQL.Inputs;

public sealed record LoginInputType
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}