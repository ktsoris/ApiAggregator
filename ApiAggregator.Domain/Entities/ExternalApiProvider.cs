namespace ApiAggregator.Domain.Entities;

public sealed class ExternalApiProvider
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string BaseUrl { get; private set; } = default!;
    public bool IsEnabled { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private ExternalApiProvider() { }

    public static ExternalApiProvider Create(string name, string baseUrl)
    {
        return new ExternalApiProvider
        {
            Id = Guid.NewGuid(),
            Name = name,
            BaseUrl = baseUrl,
            IsEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}