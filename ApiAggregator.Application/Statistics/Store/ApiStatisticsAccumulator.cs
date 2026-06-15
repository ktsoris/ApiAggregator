namespace ApiAggregator.Application.Statistics.Store;

internal sealed class ApiStatisticsAccumulator
{
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private long _totalResponseTimeMs;
    private long _fastCount;
    private long _averageCount;
    private long _slowCount;

    public void Record(bool success, long responseTimeMs)
    {
        Interlocked.Increment(ref _totalRequests);
        Interlocked.Add(ref _totalResponseTimeMs, responseTimeMs);

        if (success)
        {
            Interlocked.Increment(ref _successfulRequests);
        }
        else
        {
            Interlocked.Increment(ref _failedRequests);
        }

        if (responseTimeMs < 100)
        {
            Interlocked.Increment(ref _fastCount);
        }
        else if (responseTimeMs <= 200)
        {
            Interlocked.Increment(ref _averageCount);
        }
        else
        {
            Interlocked.Increment(ref _slowCount);
        }
    }

    public long TotalRequests => Interlocked.Read(ref _totalRequests);
    public long SuccessfulRequests => Interlocked.Read(ref _successfulRequests);
    public long FailedRequests => Interlocked.Read(ref _failedRequests);
    public long TotalResponseTimeMs => Interlocked.Read(ref _totalResponseTimeMs);
    public long FastCount => Interlocked.Read(ref _fastCount);
    public long AverageCount => Interlocked.Read(ref _averageCount);
    public long SlowCount => Interlocked.Read(ref _slowCount);

    public double AverageResponseTimeMs
    {
        get
        {
            var total = TotalRequests;
            return total == 0 ? 0 : Math.Round((double)TotalResponseTimeMs / total, 2);
        }
    }
}