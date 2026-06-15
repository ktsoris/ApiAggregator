using System.Net;
using System.Text.Json;
using ApiAggregator.Application.Aggregation.Sorting;

namespace ApiAggregator.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidSortFieldException ex)
        {
            _logger.LogWarning(ex, "Invalid sort field requested.");

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                error = "INVALID_SORT_FIELD",
                message = ex.Message
            });

            await context.Response.WriteAsync(payload);
        }
    }
}