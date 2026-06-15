using ApiAggregator.Application.Statistics.Models;
using ApiAggregator.Application.Statistics.Store;
using MediatR;

namespace ApiAggregator.Application.Statistics.Queries;

public sealed class GetApiStatisticsQueryHandler
    : IRequestHandler<GetApiStatisticsQuery, IReadOnlyList<ApiStatisticsDto>>
{
    private readonly IApiStatisticsStore _statisticsStore;

    public GetApiStatisticsQueryHandler(IApiStatisticsStore statisticsStore)
    {
        _statisticsStore = statisticsStore;
    }

    public Task<IReadOnlyList<ApiStatisticsDto>> Handle(
        GetApiStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_statisticsStore.GetStatistics());
    }
}