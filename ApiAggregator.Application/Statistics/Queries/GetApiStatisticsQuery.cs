using ApiAggregator.Application.Statistics.Models;
using MediatR;

namespace ApiAggregator.Application.Statistics.Queries;

public sealed record GetApiStatisticsQuery : IRequest<IReadOnlyList<ApiStatisticsDto>>;