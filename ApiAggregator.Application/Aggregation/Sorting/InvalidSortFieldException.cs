namespace ApiAggregator.Application.Aggregation.Sorting;

public sealed class InvalidSortFieldException : Exception
{
    public InvalidSortFieldException(string sortBy)
        : base($"'{sortBy}' is not a valid sort field. Valid values are: date, relevance, category, source, title.")
    {
    }
}