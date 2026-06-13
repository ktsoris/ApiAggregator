using ApiAggregator.Api.GraphQL.Inputs;
using ApiAggregator.Api.GraphQL.Types;
using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Aggregation.Queries;
using HotChocolate.Authorization;
using Mapster;
using MediatR;

namespace ApiAggregator.Api.GraphQL.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class AggregationQueries
{
    [Authorize]
    public async Task<AggregateDataResultType> AggregateData(
        AggregateDataInputType input,
        [Service] ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new AggregateDataQuery(input.Adapt<AggregateDataInput>());
        var result = await sender.Send(query, cancellationToken);
        return result.Adapt<AggregateDataResultType>();
    }
}