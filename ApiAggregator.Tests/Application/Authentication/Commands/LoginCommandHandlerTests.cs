using ApiAggregator.Application.Authentication.Commands;
using ApiAggregator.Application.Common.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ApiAggregator.Tests.Application.Authentication.Commands;

public sealed class LoginCommandHandlerTests
{
    private static IAuthSettings CreateAuthSettings(string username = "demo", string password = "demo-password")
    {
        var settings = Substitute.For<IAuthSettings>();
        settings.Username.Returns(username);
        settings.Password.Returns(password);
        return settings;
    }

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateToken("demo").Returns("fake-jwt-token");

        var handler = new LoginCommandHandler(tokenService, CreateAuthSettings());

        var result = await handler.Handle(
            new LoginCommand("demo", "demo-password"),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Token.Should().Be("fake-jwt-token");
        result.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUsernameIsInvalid()
    {
        var tokenService = Substitute.For<ITokenService>();

        var handler = new LoginCommandHandler(tokenService, CreateAuthSettings());

        var result = await handler.Handle(
            new LoginCommand("wrong-user", "demo-password"),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        var tokenService = Substitute.For<ITokenService>();

        var handler = new LoginCommandHandler(tokenService, CreateAuthSettings());

        var result = await handler.Handle(
            new LoginCommand("demo", "wrong-password"),
            CancellationToken.None);

        result.Success.Should().BeFalse();
        result.Token.Should().BeNull();
    }
}