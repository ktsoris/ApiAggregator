using System.Text;
using ApiAggregator.Application;
using ApiAggregator.Infrastructure;
using ApiAggregator.Persistence;
using ApiAggregator.Api.GraphQL.Queries;
using ApiAggregator.Api.GraphQL.Mutations;
using ApiAggregator.Api.GraphQL.Errors;
using ApiAggregator.Api.Mappings;
using ApiAggregator.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

MappingConfiguration.Configure();

var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Aggregator",
        Version = "v1",
        Description = "REST API for aggregating data from GitHub, HackerNews, and Open-Meteo with filtering, sorting, caching, fallback, and statistics."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });
});

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
    .AddErrorFilter<InvalidSortFieldExceptionFilter>()
    .AddQueryType(d => d.Name(OperationTypeNames.Query))
    .AddTypeExtension<AggregationQueries>()
    .AddTypeExtension<StatisticsQueries>()
    .AddMutationType(d => d.Name(OperationTypeNames.Mutation))
    .AddTypeExtension<AuthMutations>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

app.Run();