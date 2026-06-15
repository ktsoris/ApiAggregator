using ApiAggregator.Application.Aggregation.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Contracts;

public sealed class AggregateDataRequest
{
    [FromQuery(Name = "keyword")]
    public string? Keyword { get; init; }

    [FromQuery(Name = "category")]
    public string? Category { get; init; }

    [FromQuery(Name = "source")]
    public string? Source { get; init; }

    [FromQuery(Name = "fromDate")]
    public DateTimeOffset? FromDate { get; init; }

    [FromQuery(Name = "toDate")]
    public DateTimeOffset? ToDate { get; init; }

    [FromQuery(Name = "sortBy")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDirection")]
    public string? SortDirection { get; init; }

    [FromQuery(Name = "providers")]
    public List<string>? Providers { get; init; }

    public AggregateDataInput ToInput()
    {
        return new AggregateDataInput
        {
            Keyword = Keyword,
            Category = Category,
            Source = Source,
            FromDate = FromDate,
            ToDate = ToDate,
            SortBy = SortBy,
            SortDirection = SortDirection,
            Providers = Providers
        };
    }
}