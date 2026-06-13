namespace ApiAggregator.Domain.Entities;

public sealed class ApiPerformanceSnapshot
{
    public Guid Id { get; private set; }
    public string ApiName { get; private set; } = default!;
    public double AverageResponseTimeMs { get; private set; }
    public int TotalRequests { get; private set; }
    public int SuccessfulRequests { get; private set; }
    public int FailedRequests { get; private set; }
    public DateTimeOffset SnapshotTakenAt { get; private set; }

    private ApiPerformanceSnapshot() { }

    public static ApiPerformanceSnapshot Create(
        string apiName,
        double averageResponseTimeMs,
        int totalRequests,
        int successfulRequests,
        int failedRequests)
    {
        return new ApiPerformanceSnapshot
        {
            Id = Guid.NewGuid(),
            ApiName = apiName,
            AverageResponseTimeMs = averageResponseTimeMs,
            TotalRequests = totalRequests,
            SuccessfulRequests = successfulRequests,
            FailedRequests = failedRequests,
            SnapshotTakenAt = DateTimeOffset.UtcNow
        };
    }
}