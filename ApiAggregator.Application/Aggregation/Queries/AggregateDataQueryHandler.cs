using ApiAggregator.Application.Aggregation.Filtering;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Aggregation.Sorting;
using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Application.Statistics.Store;
using ApiAggregator.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApiAggregator.Application.Aggregation.Queries;

public sealed class AggregateDataQueryHandler
    : IRequestHandler<AggregateDataQuery, AggregateDataResult>
{
    private readonly IEnumerable<IExternalApiClient> _clients;
    private readonly IApiRequestLogRepository _logRepository;
    private readonly IAggregatedItemFilter _filter;
    private readonly IAggregatedItemSorter _sorter;
    private readonly IApiStatisticsStore _statisticsStore;
    private readonly ILogger<AggregateDataQueryHandler> _logger;

    public AggregateDataQueryHandler(
        IEnumerable<IExternalApiClient> clients,
        IApiRequestLogRepository logRepository,
        IAggregatedItemFilter filter,
        IAggregatedItemSorter sorter,
        IApiStatisticsStore statisticsStore,
        ILogger<AggregateDataQueryHandler> logger)
    {
        _clients = clients;
        _logRepository = logRepository;
        _filter = filter;
        _sorter = sorter;
        _statisticsStore = statisticsStore;
        _logger = logger;
    }

    public async Task<AggregateDataResult> Handle(
        AggregateDataQuery request,
        CancellationToken cancellationToken)
    {
        var input = request.Input;

        var activeClients = input.Providers is { Count: > 0 }
            ? _clients.Where(c => input.Providers.Contains(c.SourceName))
            : _clients;

        var tasks = activeClients
            .Select(client => client.FetchAsync(input, cancellationToken));

        var results = await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result.Success)
            {
                _statisticsStore.RecordSuccess(result.Source, result.ResponseTimeMs);
            }
            else
            {
                _statisticsStore.RecordFailure(result.Source, result.ResponseTimeMs);
            }
        }

        await TryPersistRequestLogsAsync(results, cancellationToken);

        var allItems = results
            .SelectMany(r => r.Items)
            .ToList();

        var filteredItems = _filter.Apply(allItems, input);
        var sortedItems = _sorter.Apply(filteredItems, input);

        var providers = results.Select(r => new ProviderResultDto
        {
            Source = r.Source,
            Success = r.Success,
            UsedFallback = r.UsedFallback,
            ResponseTimeMs = r.ResponseTimeMs,
            ErrorMessage = r.ErrorMessage,
            Items = r.Items
        }).ToList();

        return new AggregateDataResult
        {
            Items = sortedItems,
            Providers = providers
        };
    }

    private async Task TryPersistRequestLogsAsync(
        ExternalApiFetchResult[] results,
        CancellationToken cancellationToken)
    {
        try
        {
            foreach (var result in results)
            {
                var log = ApiRequestLog.Create(
                    result.Source,
                    result.Success,
                    result.UsedFallback,
                    result.ResponseTimeMs,
                    result.ErrorMessage);

                await _logRepository.AddAsync(log, cancellationToken);
            }

            await _logRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to persist API request logs to the database. " +
                "Aggregation result is unaffected; in-memory statistics remain accurate.");
        }
    }
}