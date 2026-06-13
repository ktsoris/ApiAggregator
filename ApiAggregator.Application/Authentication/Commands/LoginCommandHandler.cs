using ApiAggregator.Application.Common.Interfaces;
using MediatR;

namespace ApiAggregator.Application.Authentication.Commands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly ITokenService _tokenService;
    private readonly IAuthSettings _authSettings;

    public LoginCommandHandler(
        ITokenService tokenService,
        IAuthSettings authSettings)
    {
        _tokenService = tokenService;
        _authSettings = authSettings;
    }

    public Task<LoginResult> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Username != _authSettings.Username ||
            request.Password != _authSettings.Password)
        {
            return Task.FromResult(new LoginResult(false, null, null));
        }

        var token = _tokenService.GenerateToken(request.Username);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        return Task.FromResult(new LoginResult(true, token, expiresAt));
    }
}