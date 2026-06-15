using ApiAggregator.Application.Aggregation.Sorting;
using HotChocolate;
using HotChocolate.Execution;

namespace ApiAggregator.Api.GraphQL.Errors;

public sealed class InvalidSortFieldExceptionFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is InvalidSortFieldException ex)
        {
            return error
                .WithMessage(ex.Message)
                .WithCode("INVALID_SORT_FIELD");
        }

        return error;
    }
}