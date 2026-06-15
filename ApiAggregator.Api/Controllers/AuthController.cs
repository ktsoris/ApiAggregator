using ApiAggregator.Api.Contracts;
using ApiAggregator.Application.Authentication.Commands;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiAggregator.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="request">Username and password.</param>
    /// <returns>Access token and expiry, or success: false if invalid.</returns>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var result = await _sender.Send(command, cancellationToken);

        return Ok(result.Adapt<LoginResponse>());
    }
}