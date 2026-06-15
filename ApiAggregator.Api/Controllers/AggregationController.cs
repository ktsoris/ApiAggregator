using ApiAggregator.Api.Contracts;
using ApiAggregator.Application.Aggregation.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/aggregation")]
public sealed class AggregationController : ControllerBase
{
    private readonly ISender _sender;

    public AggregationController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Aggregates data from GitHub, HackerNews, and Open-Meteo with optional filtering and sorting.
    /// </summary>
    /// <param name="request">Filter and sort options.</param>
    /// <returns>Unified items and per-provider status metadata.</returns>
    [HttpGet]
    public async Task<ActionResult<AggregateDataResponse>> Get(
        [FromQuery] AggregateDataRequest request,
        CancellationToken cancellationToken)
    {
        var query = new AggregateDataQuery(request.ToInput());
        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Adapt<AggregateDataResponse>());
    }
}