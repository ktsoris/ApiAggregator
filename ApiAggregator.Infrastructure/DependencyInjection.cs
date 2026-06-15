using ApiAggregator.Application.Common.Interfaces;
using ApiAggregator.Infrastructure.AI;
using ApiAggregator.Infrastructure.Auth;
using ApiAggregator.Infrastructure.Clients.GitHub;
using ApiAggregator.Infrastructure.Clients.HackerNews;
using ApiAggregator.Infrastructure.Clients.OpenMeteo;
using ApiAggregator.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddHttpClient<GitHubApiClient>();
        services.AddHttpClient<HackerNewsApiClient>();
        services.AddHttpClient<OpenMeteoApiClient>();

        services.AddScoped<IExternalApiClient, GitHubApiClient>();
        services.AddScoped<IExternalApiClient, HackerNewsApiClient>();
        services.AddScoped<IExternalApiClient, OpenMeteoApiClient>();

        services.AddScoped<ITokenService, TokenService>();

        services.Configure<AuthSettings>(configuration.GetSection("Auth"));
        services.AddSingleton<IAuthSettings>(sp =>
            sp.GetRequiredService<IOptions<AuthSettings>>().Value);

        services.Configure<OpenAiSettings>(configuration.GetSection("OpenAI"));
        services.AddHttpClient<AiErrorEnricher>();
        services.AddScoped<IAiErrorEnricher, AiErrorEnricher>();

        return services;
    }
}