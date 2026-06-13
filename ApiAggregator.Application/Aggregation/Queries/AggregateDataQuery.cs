using ApiAggregator.Application.Aggregation.Models;
using MediatR;

namespace ApiAggregator.Application.Aggregation.Queries;

public sealed record AggregateDataQuery(AggregateDataInput Input)
    : IRequest<AggregateDataResult>;