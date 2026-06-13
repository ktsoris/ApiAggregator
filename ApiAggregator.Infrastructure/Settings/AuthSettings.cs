using ApiAggregator.Application.Common.Interfaces;

namespace ApiAggregator.Infrastructure.Settings;

public sealed class AuthSettings : IAuthSettings
{
    public string Username { get; init; } = default!;
    public string Password { get; init; } = default!;
}