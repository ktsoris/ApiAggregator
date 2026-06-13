using MediatR;

namespace ApiAggregator.Application.Authentication.Commands;

public sealed record LoginCommand(string Username, string Password)
    : IRequest<LoginResult>;

public sealed record LoginResult(bool Success, string? Token, DateTimeOffset? ExpiresAt);