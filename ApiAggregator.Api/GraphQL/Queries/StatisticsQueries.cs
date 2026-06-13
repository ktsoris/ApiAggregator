using ApiAggregator.Api.GraphQL.Types;
using ApiAggregator.Application.Statistics.Queries;
using Mapster;
using MediatR;

namespace ApiAggregator.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class StatisticsQueries
{
    public async Task<IReadOnlyList<ApiStatisticsType>> ApiStatistics(
        [Service] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetApiStatisticsQuery(),
            cancellationToken);

        return result.Adapt<List<ApiStatisticsType>>();
    }
}