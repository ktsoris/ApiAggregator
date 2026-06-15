namespace ApiAggregator.Application.Common.Interfaces;

public interface IAiErrorEnricher
{
    Task<string> EnrichAsync(
        string providerName,
        string rawError,
        CancellationToken cancellationToken);
}