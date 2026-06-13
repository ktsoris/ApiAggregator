using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Domain.Entities;
using MediatR;

namespace ApiAggregator.Application.Aggregation.Queries;

public sealed class AggregateDataQueryHandler
    : IRequestHandler<AggregateDataQuery, AggregateDataResult>
{
    private readonly IEnumerable<IExternalApiClient> _clients;
    private readonly IApiRequestLogRepository _logRepository;

    public AggregateDataQueryHandler(
        IEnumerable<IExternalApiClient> clients,
        IApiRequestLogRepository logRepository)
    {
        _clients = clients;
        _logRepository = logRepository;
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
            var log = ApiRequestLog.Create(
                result.Source,
                result.Success,
                result.UsedFallback,
                result.ResponseTimeMs,
                result.ErrorMessage);

            await _logRepository.AddAsync(log, cancellationToken);
        }

        await _logRepository.SaveChangesAsync(cancellationToken);

        var allItems = results
            .SelectMany(r => r.Items)
            .ToList();

        if (!string.IsNullOrWhiteSpace(input.SortBy))
        {
            allItems = input.SortBy.ToLowerInvariant() switch
            {
                "date" => allItems.OrderByDescending(i => i.PublishedAt).ToList(),
                "relevance" => allItems.OrderByDescending(i => i.RelevanceScore).ToList(),
                "source" => allItems.OrderBy(i => i.Source).ToList(),
                _ => allItems
            };
        }

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
            Items = allItems,
            Providers = providers
        };
    }
}