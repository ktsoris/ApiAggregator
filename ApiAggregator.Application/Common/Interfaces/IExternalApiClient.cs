using ApiAggregator.Application.Aggregation.Models;

namespace ApiAggregator.Application.Common.Interfaces;

public interface IExternalApiClient
{
    string SourceName { get; }

    Task<ExternalApiFetchResult> FetchAsync(
        AggregateDataInput input,
        CancellationToken cancellationToken);
}