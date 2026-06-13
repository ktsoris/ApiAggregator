using ApiAggregator.Api.GraphQL.Inputs;
using ApiAggregator.Api.GraphQL.Types;
using ApiAggregator.Application.Authentication.Commands;
using Mapster;
using MediatR;

namespace ApiAggregator.Api.GraphQL.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class AuthMutations
{
    public async Task<LoginResultType> Login(
        LoginInputType input,
        [Service] ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(input.Username, input.Password);
        var result = await sender.Send(command, cancellationToken);
        return result.Adapt<LoginResultType>();
    }
}