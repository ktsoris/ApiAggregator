using ApiAggregator.Application.Aggregation.Filtering;
using ApiAggregator.Application.Aggregation.Sorting;
using ApiAggregator.Application.Statistics.Store;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAggregator.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IAggregatedItemFilter, AggregatedItemFilter>();
        services.AddScoped<IAggregatedItemSorter, AggregatedItemSorter>();
        services.AddSingleton<IApiStatisticsStore, InMemoryApiStatisticsStore>();

        return services;
    }
}