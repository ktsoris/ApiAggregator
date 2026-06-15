namespace ApiAggregator.Infrastructure.Settings;

public sealed class OpenAiSettings
{
    public string ApiKey { get; init; } = default!;
    public string Model { get; init; } = default!;
}