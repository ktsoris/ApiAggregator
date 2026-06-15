using ApiAggregator.Api.Contracts;
using ApiAggregator.Application.Statistics.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/statistics")]
public sealed class StatisticsController : ControllerBase
{
    private readonly ISender _sender;

    public StatisticsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Returns per-provider request statistics from the in-memory statistics store.
    /// </summary>
    /// <returns>Request counts, average response time, and performance buckets per provider.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiStatisticsListResponse>> Get(
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetApiStatisticsQuery(), cancellationToken);

        return Ok(new ApiStatisticsListResponse
        {
            Apis = result.Adapt<List<ApiStatisticsResponse>>()
        });
    }
}