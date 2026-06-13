using ApiAggregator.Application.Aggregation.Models;
using ApiAggregator.Application.Statistics.Models;
using ApiAggregator.Api.GraphQL.Types;
using ApiAggregator.Api.GraphQL.Inputs;
using Mapster;
using ApiAggregator.Application.Authentication.Commands;

namespace ApiAggregator.Api.Mappings;

public static class MappingConfiguration
{
    public static void Configure()
    {
        TypeAdapterConfig<AggregateDataInputType, AggregateDataInput>
            .NewConfig();

        TypeAdapterConfig<AggregatedItemDto, AggregatedItemType>
            .NewConfig();

        TypeAdapterConfig<ProviderResultDto, ProviderStatusType>
            .NewConfig();

        TypeAdapterConfig<AggregateDataResult, AggregateDataResultType>
            .NewConfig()
            .Map(dest => dest.Items, src => src.Items.Adapt<List<AggregatedItemType>>())
            .Map(dest => dest.Providers, src => src.Providers.Adapt<List<ProviderStatusType>>());

        TypeAdapterConfig<PerformanceBucketsDto, PerformanceBucketsType>
            .NewConfig();

        TypeAdapterConfig<ApiStatisticsDto, ApiStatisticsType>
            .NewConfig();

        TypeAdapterConfig<LoginInputType, LoginCommand>
            .NewConfig();

        TypeAdapterConfig<LoginResult, LoginResultType>
            .NewConfig();
    }
}