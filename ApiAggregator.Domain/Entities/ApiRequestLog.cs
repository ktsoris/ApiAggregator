using ApiAggregator.Domain.Enums;

namespace ApiAggregator.Domain.Entities;

public sealed class ApiRequestLog
{
    public Guid Id { get; private set; }
    public string ApiName { get; private set; } = default!;
    public bool Success { get; private set; }
    public bool UsedFallback { get; private set; }
    public long ResponseTimeMs { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset RequestedAt { get; private set; }
    public ProviderStatus Status { get; private set; }

    private ApiRequestLog() { }

    public static ApiRequestLog Create(
        string apiName,
        bool success,
        bool usedFallback,
        long responseTimeMs,
        string? errorMessage)
    {
        return new ApiRequestLog
        {
            Id = Guid.NewGuid(),
            ApiName = apiName,
            Success = success,
            UsedFallback = usedFallback,
            ResponseTimeMs = responseTimeMs,
            ErrorMessage = errorMessage,
            RequestedAt = DateTimeOffset.UtcNow,
            Status = success
                ? ProviderStatus.Success
                : usedFallback
                    ? ProviderStatus.Fallback
                    : ProviderStatus.Failed
        };
    }
}