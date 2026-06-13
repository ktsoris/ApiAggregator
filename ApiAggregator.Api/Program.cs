using System.Text;
using ApiAggregator.Application;
using ApiAggregator.Infrastructure;
using ApiAggregator.Persistence;
using ApiAggregator.Api.GraphQL.Queries;
using ApiAggregator.Api.GraphQL.Mutations;
using ApiAggregator.Api.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

MappingConfiguration.Configure();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType(d => d.Name(OperationTypeNames.Query))
    .AddTypeExtension<AggregationQueries>()
    .AddTypeExtension<StatisticsQueries>()
    .AddMutationType(d => d.Name(OperationTypeNames.Mutation))
    .AddTypeExtension<AuthMutations>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL();

app.Run();