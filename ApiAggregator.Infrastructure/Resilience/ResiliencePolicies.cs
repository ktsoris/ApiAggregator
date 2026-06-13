using Polly;
using Polly.Extensions.Http;

namespace ApiAggregator.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(2, retryAttempt =>
                TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt)));
    }

    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(5));
    }
}