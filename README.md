# API Aggregator Assignment

This project is a .NET-based API aggregation service built with ASP.NET Core. It consolidates data from multiple external APIs and exposes the aggregated result through both REST endpoints and Hot Chocolate GraphQL.

REST is the primary, assignment-compliant API surface. GraphQL is provided as an additional interface that demonstrates further technical depth, and both surfaces call the exact same application logic.

---

## Project Overview

The goal of the project is to fetch data from multiple external APIs, normalize the results into a shared response model, apply filtering and sorting, cache responses, record request statistics, and expose the data through a unified API.

The solution currently integrates with:

- **GitHub API** — repository search results
- **Hacker News API** — top stories, optionally filtered by keyword
- **Open-Meteo API** — current weather for Athens

All three providers are called in parallel using `Task.WhenAll` to reduce total response time. A failure in one provider never fails the overall request — each provider returns its own success/failure metadata alongside any items it managed to retrieve.

The project also includes an AI-assisted error enrichment layer: when a provider fails and no cached fallback is available, the raw technical exception is rewritten into a short, calm, user-facing message via OpenAI before being returned to the client.

---

## Assignment Requirement Coverage

| Requirement | Implemented? | Notes |
|---|---:|---|
| Develop a .NET API aggregation service using ASP.NET Core | Yes | ASP.NET Core with REST controllers and Hot Chocolate GraphQL. |
| Easy integration of new APIs | Yes | New providers implement `IExternalApiClient` and are registered in DI; the aggregation handler does not change. |
| Fetch from at least three APIs simultaneously | Yes | GitHub, Hacker News and Open-Meteo are fetched in parallel using `Task.WhenAll`. |
| Unified API endpoint | Yes | REST `GET /api/aggregation` and GraphQL `aggregateData`, both backed by `AggregateDataQueryHandler`. |
| Filtering and sorting | Yes | Supported in both REST and GraphQL through a shared `AggregateDataInput` model, `IAggregatedItemFilter` and `IAggregatedItemSorter`. |
| Error handling and fallback | Yes | Per-provider try/catch, cache-based fallback, and AI-based error message enrichment as a last resort. |
| Unit tests | Yes | Tests cover filtering, sorting, the statistics store, the aggregation handler, and authentication. |
| Documentation | Yes | This README, Swagger, GraphQL Nitro, and a Postman collection. |
| Caching | Yes | `IApiResponseCache` and `IApiCacheKeyFactory`, with an in-memory implementation and a documented Redis-ready alternative. |
| Parallelism | Yes | All external provider calls execute concurrently via `Task.WhenAll`. |
| Request statistics | Yes | Thread-safe in-memory statistics store with fast/average/slow performance buckets. |
| JWT authentication | Yes | JWT bearer authentication protects REST controllers and GraphQL operations. |
| AI-assisted error messages | Yes | OpenAI rewrites raw provider exceptions into user-friendly messages when no cache fallback exists. |

---

## Solution Architecture

```text
ApiAggregator
 ├── ApiAggregator.Api
 ├── ApiAggregator.Application
 ├── ApiAggregator.Domain
 ├── ApiAggregator.Infrastructure
 ├── ApiAggregator.Persistence
 ├── ApiAggregator.Contracts
 └── ApiAggregator.Tests
```

### ApiAggregator.Api

REST controllers, Hot Chocolate GraphQL queries/mutations, JWT authentication setup, Swagger configuration, GraphQL error filters, and exception-handling middleware. Contains no business logic — every controller and resolver delegates to MediatR.

### ApiAggregator.Application

MediatR commands and queries, their handlers, application DTOs, the filtering and sorting services, the in-memory statistics store, and the application-level abstractions (`IExternalApiClient`, `IApiResponseCache`, `IApiCacheKeyFactory`, `IApiRequestLogRepository`, `ITokenService`, `IAuthSettings`, `IAiErrorEnricher`).

### ApiAggregator.Domain

Core domain entities and enums: `AggregatedItem`, `ExternalApiProvider`, `ApiRequestLog`, `ApiPerformanceSnapshot`, and `ProviderStatus`.

### ApiAggregator.Infrastructure

External API clients (GitHub, Hacker News, Open-Meteo), the memory-based and Redis-ready cache implementations, the cache key factory, JWT token generation, and the OpenAI-backed AI error enricher.

### ApiAggregator.Persistence

EF Core `DbContext`, PostgreSQL configuration via Npgsql, entity configurations, migrations, and the `ApiRequestLog` repository used for optional audit history.

### ApiAggregator.Contracts

Reserved for shared contracts between services if the solution is split further in the future. Currently empty in this version.

### ApiAggregator.Tests

Unit tests for filtering, sorting, the in-memory statistics store, the aggregation handler (parallelism, fallback isolation, provider filtering, statistics recording, graceful EF degradation), and the login command handler.

---

## REST and GraphQL Interfaces

The assignment requires a standard API aggregation service, so REST controllers are the primary API surface.

Hot Chocolate GraphQL was added as an additional interface because it provides:

- Strong schema support
- Typed queries and mutations
- Built-in request validation
- A structured, schema-aware error model
- Flexible, client-driven response shapes
- A clean developer experience through Nitro

**Both REST controllers and GraphQL resolvers send the same MediatR commands and queries** (`LoginCommand`, `AggregateDataQuery`, `GetApiStatisticsQuery`) to the same handlers in `ApiAggregator.Application`. This avoids duplicating business logic and guarantees that REST and GraphQL behave identically.

| Concern | REST | GraphQL |
|---|---|---|
| Base URL | `http://localhost:5025` | `https://localhost:7209/graphql/` |
| Login | `POST /api/auth/login` | `mutation { login(...) }` |
| Aggregation | `GET /api/aggregation` | `query { aggregateData(...) }` |
| Statistics | `GET /api/statistics` | `query { apiStatistics }` |
| Docs/UI | Swagger | GraphQL Nitro |

---

## Swagger

The REST API is documented through Swagger/OpenAPI.

Swagger UI is accessible at:

```text
http://localhost:5025/swagger
```

Use the **Authorize** button in Swagger UI to paste a `Bearer <token>` value obtained from `POST /api/auth/login`, then test the aggregation and statistics endpoints directly from the browser.

---

## GraphQL with Hot Chocolate Nitro

Hot Chocolate provides the GraphQL server implementation. Nitro provides an interactive interface for exploring and executing GraphQL operations.

Nitro is accessible at:

```text
https://localhost:7209/graphql/
```

> **Note:** if Nitro's hosted UI fails to load (e.g. due to a CDN/network issue), the GraphQL endpoint itself still works and can be exercised directly via Postman by sending a `POST` request to the same URL with a JSON body containing `query` and `variables`.

### Sample query — Aggregate Data

```graphql
query AggregateData($input: AggregateDataInputTypeInput!) {
  aggregateData(input: $input) {
    items {
      source
      title
      description
      category
      publishedAt
      relevanceScore
      url
    }
    providers {
      source
      success
      usedFallback
      responseTimeMs
      errorMessage
    }
  }
}
```

### Sample variables

```json
{
  "input": {
    "keyword": "dotnet",
    "category": null,
    "source": null,
    "fromDate": null,
    "toDate": null,
    "sortBy": "date",
    "sortDirection": "desc"
  }
}
```

> **Naming note:** Hot Chocolate appends `Input` to GraphQL input type names by convention. Our C# type `AggregateDataInputType` becomes `AggregateDataInputTypeInput` in the actual schema, and `LoginInputType` becomes `LoginInputTypeInput`. Use these exact names as the GraphQL variable types.

---

## Authentication

The API is secured using JWT bearer authentication. The login operation returns an access token which must be supplied when calling protected REST or GraphQL operations.

### REST login

```text
POST /api/auth/login
```

Request:

```json
{
  "username": "demo",
  "password": "demo-password"
}
```

Response:

```json
{
  "success": true,
  "accessToken": "jwt-token-here",
  "expiresAt": "2026-06-15T12:00:00Z"
}
```

### GraphQL login

```graphql
mutation AuthenticateUser($input: LoginInputTypeInput!) {
  login(input: $input) {
    success
    token
    expiresAt
  }
}
```

```json
{
  "input": {
    "username": "demo",
    "password": "demo-password"
  }
}
```

### Using the token

Protected endpoints require:

```text
Authorization: Bearer {accessToken}
```

The same token can be used in Postman, Swagger, and GraphQL Nitro. Protected operations:

- `GET /api/aggregation` / GraphQL `aggregateData`
- `GET /api/statistics` / GraphQL `apiStatistics`

Tokens expire after the number of minutes configured in `Jwt:ExpiryMinutes` (60 minutes by default).

Credentials are configured under `Auth:Username` / `Auth:Password` in `appsettings.json` — for the assignment, a single demo user is sufficient and avoids overcomplicating identity management.

---

## User Secrets

For local development, secrets are stored using .NET User Secrets rather than committed to source control. **User Secrets are per-machine** — if you clone this repository onto a new machine, you must configure them again.

Initialize and set secrets:

```bash
dotnet user-secrets init --project ApiAggregator.Api

dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY" --project ApiAggregator.Api
dotnet user-secrets set "OpenAI:Model" "gpt-4o-mini" --project ApiAggregator.Api
dotnet user-secrets set "Jwt:Secret" "YOUR_LOCAL_DEV_JWT_SECRET" --project ApiAggregator.Api
```

Or via Visual Studio: right-click `ApiAggregator.Api` → **Manage User Secrets**, and paste:

```json
{
  "OpenAI": {
    "ApiKey": "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",
    "Model": "gpt-4o-mini"
  },
  "Jwt": {
    "Secret": "this-is-a-super-secret-key-for-dev-only-change-in-prod"
  }
}
```

**Never commit real API keys, JWT secrets, or credentials to Git.** If a real key has ever been pasted into a chat, document, or shared system, rotate it immediately on the provider's dashboard.

---

## PostgreSQL and EF Core

The solution uses EF Core with PostgreSQL (via Npgsql) for persistence. PostgreSQL stores the `ApiRequestLog` audit trail — a history of every provider call, its success/failure status, response time, and error message.

### Running PostgreSQL with Docker

A `docker-compose.yml` is provided at the solution root:

```yaml
services:
  postgres:
    image: postgres:16
    container_name: apiaggregator_postgres
    environment:
      POSTGRES_DB: apiaggregator
      POSTGRES_USER: apiaggregator_user
      POSTGRES_PASSWORD: apiaggregator_pass
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Start it:

```bash
docker compose up -d
```

Confirm it's running:

```bash
docker ps
```

### Important — PostgreSQL is optional at runtime

The project originally used SQL-backed storage for request statistics. However, the assignment explicitly allows statistics source data to be stored in memory, so **the in-memory statistics store is now the primary source for `/api/statistics` and `apiStatistics`**. EF Core / PostgreSQL is used only for the optional `ApiRequestLog` audit history.

See [PostgreSQL Logging Robustness](#postgresql-logging-robustness) below — the application runs correctly even if PostgreSQL is not started.

---

## EF Core Migrations

```bash
dotnet ef migrations add InitialCreate \
  --project ApiAggregator.Persistence \
  --startup-project ApiAggregator.Api

dotnet ef database update \
  --project ApiAggregator.Persistence \
  --startup-project ApiAggregator.Api
```

Make sure the PostgreSQL Docker container is running before applying migrations — otherwise `dotnet ef database update` will fail to connect.

---

## PostgreSQL Logging Robustness

During development, it became clear that aggregation itself could succeed in full — external API calls, fallback logic, and in-memory statistics all worked correctly — while the request still failed overall, because EF Core's `SaveChangesAsync` call (writing the `ApiRequestLog` audit entries) threw when PostgreSQL was unreachable.

This was a robustness gap: an optional audit trail should never be able to take down the core aggregation feature.

**Fix applied:** EF Core audit logging is wrapped in its own try/catch inside `AggregateDataQueryHandler`. If persistence fails, a warning is logged via `ILogger<AggregateDataQueryHandler>` and the request continues normally.

```csharp
private async Task TryPersistRequestLogsAsync(
    ExternalApiFetchResult[] results,
    CancellationToken cancellationToken)
{
    try
    {
        foreach (var result in results)
        {
            var log = ApiRequestLog.Create(
                result.Source, result.Success, result.UsedFallback,
                result.ResponseTimeMs, result.ErrorMessage);

            await _logRepository.AddAsync(log, cancellationToken);
        }

        await _logRepository.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex,
            "Failed to persist API request logs to the database. " +
            "Aggregation result is unaffected; in-memory statistics remain accurate.");
    }
}
```

**Result:** if PostgreSQL is unavailable, `/api/aggregation` and `aggregateData` still succeed. In-memory statistics are recorded as normal; only the optional audit history in PostgreSQL is skipped, with a warning logged.

---

## Request Statistics

The statistics feature uses a thread-safe in-memory store (`InMemoryApiStatisticsStore`, registered as a singleton, backed by `ConcurrentDictionary` and `Interlocked` counters). This matches the assignment's allowance that statistics source data may be stored in memory, and means the statistics endpoint has **no dependency on PostgreSQL**.

Each provider's statistics include:

- API name
- Total requests
- Successful requests
- Failed requests
- Average response time (ms)
- Fast / Average / Slow bucket counts

Performance buckets:

| Bucket | Range |
|---|---|
| Fast | < 100ms |
| Average | 100ms – 200ms |
| Slow | > 200ms |

### REST

```text
GET /api/statistics
```

### GraphQL

```graphql
query GetApiStatistics {
  apiStatistics {
    apiName
    totalRequests
    successfulRequests
    failedRequests
    averageResponseTimeMs
    buckets {
      fast
      average
      slow
    }
  }
}
```

### Example — statistics evolving over multiple requests

After two GitHub requests:

```json
{
  "apis": [
    {
      "apiName": "GitHub",
      "totalRequests": 2,
      "successfulRequests": 2,
      "failedRequests": 0,
      "averageResponseTimeMs": 486.5,
      "buckets": { "fast": 1, "average": 0, "slow": 1 }
    }
  ]
}
```

After a third, faster GitHub request:

```json
{
  "apis": [
    {
      "apiName": "GitHub",
      "totalRequests": 3,
      "successfulRequests": 3,
      "failedRequests": 0,
      "averageResponseTimeMs": 345.33,
      "buckets": { "fast": 2, "average": 0, "slow": 1 }
    }
  ]
}
```

The average response time and bucket counts update as each new request is recorded — the third, faster request pulled the average down from 486.5ms to 345.33ms and moved the fast-bucket count from 1 to 2.

Statistics reset whenever the application restarts, since they are held in memory (see [Known Limitations](#known-limitations)).

---

## External API Clients

The current provider implementations are:

- `GitHubApiClient` — searches GitHub repositories by keyword (defaults to `dotnet`)
- `HackerNewsApiClient` — fetches the top 10 Hacker News stories, optionally filtered by keyword
- `OpenMeteoApiClient` — fetches current weather for Athens

All three implement `IExternalApiClient` and use:

- `IApiResponseCache` — get/set cached provider results
- `IApiCacheKeyFactory` — builds a consistent cache key per provider and input

Each client is responsible only for calling its external API, mapping the response into `AggregatedItemDto`, and returning provider metadata (`Success`, `UsedFallback`, `ResponseTimeMs`, `ErrorMessage`). Clients do **not** perform global aggregation, filtering, sorting, authentication, or statistics calculation — those concerns live in the application layer.

---

## Adding a New API Provider

To add a new external API provider:

1. Create a new client class in `ApiAggregator.Infrastructure/Clients/<ProviderName>/`.
2. Implement `IExternalApiClient` (`SourceName` property + `FetchAsync`).
3. Add response DTOs matching the external API's JSON shape.
4. Map the external response into `AggregatedItemDto`.
5. Use `IApiResponseCache` and `IApiCacheKeyFactory` for caching and fallback, and `IAiErrorEnricher` for the final-fallback error message.
6. Add provider-specific configuration to `appsettings.json` if needed (base URL, API key, etc.).
7. Register the client in `Infrastructure/DependencyInjection.cs`:
   ```csharp
   services.AddHttpClient<YourNewApiClient>();
   services.AddScoped<IExternalApiClient, YourNewApiClient>();
   ```
8. Add unit tests for success, failure/fallback, and response mapping.

**The aggregation handler does not need to change.** `AggregateDataQueryHandler` receives `IEnumerable<IExternalApiClient>` via DI, so any registered client is automatically included in `Task.WhenAll` and the unified response.

---

## Filtering and Sorting

Filtering and sorting are available through both REST and GraphQL, using the same shared `AggregateDataInput` model and the same `AggregateDataQueryHandler`.

### Supported filters (`IAggregatedItemFilter`)

- `keyword` — matches against item title or description
- `category` — exact match (case-insensitive)
- `source` — exact match (case-insensitive), e.g. `GitHub`, `HackerNews`, `OpenMeteo`
- `fromDate` / `toDate` — filters by `publishedAt`

### Supported sorting (`IAggregatedItemSorter`)

- `date`
- `relevance`
- `category`
- `source`
- `title`

Sort direction: `asc` (default) or `desc`.

An invalid `sortBy` value throws `InvalidSortFieldException`, which is translated into:
- a `400 Bad Request` with an `INVALID_SORT_FIELD` error code on REST (via `ExceptionHandlingMiddleware`)
- a structured GraphQL error with code `INVALID_SORT_FIELD` (via `InvalidSortFieldExceptionFilter`)

### REST example

```text
GET /api/aggregation?keyword=dotnet&category=technology&sortBy=date&sortDirection=desc
```

### GraphQL example

```graphql
query AggregateData($input: AggregateDataInputTypeInput!) {
  aggregateData(input: $input) {
    items {
      source
      title
      category
      publishedAt
      relevanceScore
    }
  }
}
```

```json
{
  "input": {
    "keyword": "dotnet",
    "category": "technology",
    "sortBy": "date",
    "sortDirection": "desc"
  }
}
```

---

## Caching

Caching reduces redundant external API calls and improves response times.

The application depends on two abstractions (defined in `ApiAggregator.Application.Common.Caching`):

- `IApiResponseCache` — `GetAsync<T>` / `SetAsync<T>`
- `IApiCacheKeyFactory` — `CreateProviderCacheKey(providerName, input)`

The default implementation, `MemoryApiResponseCache`, wraps `IMemoryCache`. Because the providers and the aggregation handler depend only on the abstraction, **the design is Redis-ready**: a `RedisApiResponseCache` implementation (using `IDistributedCache`) is included and documented in code, ready to be activated by installing `Microsoft.Extensions.Caching.StackExchangeRedis`, registering `AddStackExchangeRedisCache`, and swapping the `IApiResponseCache` registration — no other code changes are required.

### Cache TTLs

| Provider | Cache TTL |
|---|---:|
| GitHub | 5 minutes |
| Hacker News | 5 minutes |
| Open-Meteo | 10 minutes |

### Fallback flow

1. Try the live external API call.
2. If it succeeds, cache the result and return it.
3. If it fails, check the cache for a previous successful result for the same key.
4. If a cached result exists, return it with `usedFallback: true` and an explanatory `errorMessage`.
5. If no cached result exists, fall through to AI-assisted error enrichment (see below) and return an empty result with `success: false`.

A provider failure never fails the overall aggregation request — it is always isolated to that provider's entry in the `providers` array.

---

## Parallelism

`AggregateDataQueryHandler` executes all enabled provider calls concurrently:

```csharp
var tasks = activeClients
    .Select(client => client.FetchAsync(input, cancellationToken));

var results = await Task.WhenAll(tasks);
```

Rather than calling GitHub, Hacker News, and Open-Meteo sequentially, all three calls start together and the handler waits for all of them via `Task.WhenAll`. Total response time is therefore bounded by the **slowest** provider, not the sum of all providers. All external calls are asynchronous and propagate `CancellationToken`.

---

## Error Handling and Fallback

### REST

`ExceptionHandlingMiddleware` catches `InvalidSortFieldException` and returns a `400 Bad Request` with a structured JSON error body (`error: "INVALID_SORT_FIELD"`, plus a message).

### GraphQL

Hot Chocolate's error pipeline is extended with `InvalidSortFieldExceptionFilter`, which converts the same exception into a GraphQL error with code `INVALID_SORT_FIELD` — one of the reasons GraphQL was kept as an additional interface, since it provides a structured, schema-aware error model out of the box.

### External API providers

Each provider client wraps its external call in a try/catch. A failure in one provider never fails the overall request. The provider's entry in the `providers` array always includes:

- `source`
- `success`
- `usedFallback`
- `responseTimeMs`
- `errorMessage`

This handles transient failures such as timeouts, DNS/network errors, rate limiting, and HTTP 5xx/4xx responses from the external API.

---

## AI Agent and Error Message Enrichment

The project includes an AI-assisted error enrichment layer (`IAiErrorEnricher`, implemented by `AiErrorEnricher` using the OpenAI Chat Completions API with `gpt-4o-mini`).

**When it triggers:** only when a provider's live call fails **and** no cached fallback data is available for that key. In normal operation this is rare — GitHub, Hacker News, and Open-Meteo are public, reliable APIs, and a warm cache from a previous successful call usually covers transient failures.

**What it does:** takes the raw technical exception (e.g. a `SocketException` with a DNS resolution failure) and asks OpenAI to rewrite it as a short (max 2 sentences), calm, professional message that doesn't expose internal details — suitable for returning directly to an API consumer.

**Example — verified end-to-end:**

Raw exception (from logs):
```text
System.Net.Http.HttpRequestException: No such host is known. (api.github.invalid:443)
 ---> System.Net.Sockets.SocketException (11001): No such host is known.
```

AI-enriched `errorMessage` returned to the client:
```text
"We are unable to connect to GitHub at the moment. Please check your internet connection or try again later."
```

**Resilience:** if the OpenAI call itself fails (e.g. missing/invalid API key, network issue), `AiErrorEnricher` catches the exception, logs a warning, and falls back to a generic message (`"The {provider} provider is currently unavailable."`) — so a misconfigured AI integration never breaks the aggregation response.

### Testing this locally

To intentionally trigger the AI enrichment path:

1. Temporarily change a provider's base URL to something unresolvable, e.g. in `GitHubApiClient`:
   ```csharp
   _httpClient.BaseAddress = new Uri("https://api.github.invalid/");
   ```
2. Use a **new, never-cached** query (e.g. a keyword you haven't queried before) so no cached fallback exists.
3. Call `aggregateData` / `GET /api/aggregation` and inspect the `errorMessage` for that provider.
4. **Revert the URL change** afterward — this is for local failure testing only and must never be committed.

A cleaner long-term approach would be to simulate provider failure via configuration or test doubles rather than editing a hardcoded URL; this is noted as a possible future improvement.

---

## Postman Collection

The repository includes Postman collection and environment files:

- `postman/API Aggregator.postman_collection.json`
- `postman/API Aggregator - Local.postman_environment.json`

### Import and use

1. Open Postman.
2. Import both JSON files (**Import** → drag and drop).
3. Select the **API Aggregator - Local** environment from the top-right dropdown.
4. Run **00 - Authentication → Authenticate User** (GraphQL) or **Login (REST)** — both store the returned JWT in the `accessToken` environment variable automatically.
5. Run the protected aggregation and statistics requests — the collection-level Bearer auth applies `{{accessToken}}` automatically.

### Environment variables

| Variable | Example value |
|---|---|
| `baseUrl` | `http://localhost:5025` |
| `graphqlUrl` | `https://localhost:7209/graphql/` |
| `accessToken` | *(set automatically after login)* |
| `username` | `demo` |
| `password` | `demo-password` |

### Collection structure

```text
API Aggregator
 ├── 00 - Authentication
 │   ├── Authenticate User (GraphQL)
 │   ├── Login (REST)
 │   └── Validate Authorized Request
 │
 ├── 01 - Aggregation
 │   ├── Aggregate Data (GraphQL)
 │   ├── Aggregate Data by Keyword (GraphQL)
 │   ├── Aggregate Data Sorted by Relevance (GraphQL)
 │   ├── Aggregate Data by Provider (GraphQL)
 │   ├── Get Aggregated Data (REST)
 │   ├── Get Aggregated Data by Keyword (REST)
 │   └── Get Aggregated Data Sorted by Relevance (REST)
 │
 ├── 02 - Providers
 │   └── (Not implemented — reserved for future provider management endpoints)
 │
 ├── 03 - Statistics
 │   ├── Get API Statistics (GraphQL)
 │   └── Get API Statistics (REST)
 │
 ├── 04 - Health & Diagnostics
 │   └── (Not implemented — reserved for a future /health endpoint)
 │
 └── 99 - Examples / Edge Cases
     ├── Aggregate Data without Token
     ├── Aggregate Data with Invalid Sort Field
     └── Authenticate User with Invalid Credentials
```

The folder structure mirrors API **capabilities** rather than internal class names, so it reads naturally for someone evaluating the API from the outside.

---

## Unit Tests

The test project (`ApiAggregator.Tests`) verifies the correctness and reliability of the application layer using **xUnit**, **FluentAssertions**, and **NSubstitute**.

Covered areas:

- **Filtering** (`AggregatedItemFilterTests`) — keyword, category, source, date range filters
- **Sorting** (`AggregatedItemSorterTests`) — date, relevance, category, source, title, ascending/descending, invalid sort field
- **In-memory statistics store** (`InMemoryApiStatisticsStoreTests`) — success/failure recording, average response time, performance buckets, multi-API tracking, and **thread-safe concurrent writes**
- **Aggregation handler** (`AggregateDataQueryHandlerTests`) — calling all providers, isolating provider failures, provider status metadata, provider filtering, statistics recording, graceful degradation when EF Core logging fails, and filter/sort integration
- **Authentication** (`LoginCommandHandlerTests`) — valid credentials, invalid username, invalid password

### Running tests

```bash
dotnet test
```

With a TRX report:

```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
```

The report is generated under the test project's `TestResults` folder, e.g. `ApiAggregator.Tests/TestResults/test-results.trx`.

All 33 tests pass at the time of writing.

---

## Third-Party Packages

| Package / Tool | Purpose |
|---|---|
| ASP.NET Core | Web API host and REST controllers |
| Hot Chocolate (`HotChocolate.AspNetCore`, `.Authorization`) | GraphQL server and authorization integration |
| Hot Chocolate Nitro | GraphQL development UI |
| Swashbuckle.AspNetCore | Swagger/OpenAPI documentation |
| MediatR | Application use-case dispatching (commands/queries) |
| Mapster (`Mapster`, `Mapster.DependencyInjection`) | Object mapping between domain, DTO, GraphQL, and REST models |
| EF Core | ORM for persistence |
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL database provider |
| Microsoft.AspNetCore.Authentication.JwtBearer | JWT bearer authentication |
| System.IdentityModel.Tokens.Jwt | JWT generation |
| Microsoft.Extensions.Caching.Memory | In-memory caching (`MemoryApiResponseCache`) |
| Microsoft.Extensions.Caching.Distributed / StackExchangeRedis (optional) | Redis-ready distributed caching (`RedisApiResponseCache`, not active by default) |
| Polly / Microsoft.Extensions.Http.Polly | HTTP resilience policies (retry, timeout) |
| OpenAI Chat Completions API (via `HttpClient`) | AI-assisted error message enrichment |
| xUnit | Test framework |
| FluentAssertions | Readable test assertions |
| NSubstitute | Mocking/substitution in unit tests |

Exact package versions can be found in each project's `.csproj` file.

---

## Why MediatR Was Used

MediatR keeps REST controllers and GraphQL resolvers thin. Both API surfaces send commands and queries to the application layer rather than containing business logic directly.

Benefits:

- Clear use-case boundaries (`LoginCommand`, `AggregateDataQuery`, `GetApiStatisticsQuery`)
- Better separation of concerns between transport (REST/GraphQL) and application logic
- Easier unit testing — handlers can be tested without any HTTP/GraphQL infrastructure
- The same application logic is reused by REST and GraphQL with zero duplication
- Strong alignment with Clean Architecture's dependency rules

For example, `AggregationController.Get` and `AggregationQueries.AggregateData` both construct an `AggregateDataQuery` and send it via `ISender`, both ultimately invoking `AggregateDataQueryHandler`.

---

## Why Mapster Was Used

Mapster maps between external API DTOs, application DTOs, GraphQL types, and REST response models without manually written, repetitive mapping code.

Benefits:

- Reduces boilerplate mapping code across three layers (Application DTOs ↔ GraphQL types ↔ REST contracts)
- Keeps external provider response models (e.g. `GitHubRepository`, `HackerNewsStory`) separate from internal and API-facing models
- Centralizes mapping configuration in `MappingConfiguration.cs`
- Improves maintainability as new fields or types are added

The project never exposes external provider response models or EF Core entities directly through REST or GraphQL — only purpose-built DTOs and response/type records.

---

## Why Hot Chocolate Was Added

Hot Chocolate was added as an additional GraphQL API layer because it provides:

- Code-first GraphQL schema generation from C# types
- Strongly typed, introspectable schema
- Built-in request validation
- A structured, schema-aware GraphQL error model (used directly for `InvalidSortFieldException`)
- A developer-friendly Nitro interface for exploring and running queries
- Flexible, client-selected response shapes (clients request only the fields they need)

REST remains the primary interface because it is the assignment-compliant surface; GraphQL demonstrates an additional, production-relevant API style built on the same underlying logic.

---

## Why Request Statistics Use In-Memory Storage

The solution originally used SQL-backed (EF Core / PostgreSQL) storage for request statistics. However, the assignment explicitly allows statistics source data to be stored in memory.

The design was updated so that the primary statistics feature — `GetApiStatisticsQueryHandler` — reads from `InMemoryApiStatisticsStore`, a thread-safe singleton backed by `ConcurrentDictionary<string, ApiStatisticsAccumulator>` with `Interlocked`-based counters.

EF Core's `ApiRequestLog` table remains as optional audit/history data, written best-effort by `AggregateDataQueryHandler`, but it is **not** required for the statistics endpoint.

This also directly improves robustness (see [PostgreSQL Logging Robustness](#postgresql-logging-robustness)): the statistics endpoint and the aggregation flow do not depend on PostgreSQL being available at all.

---

## Running the Project Locally

1. Clone the repository.
2. Start PostgreSQL using Docker (optional — see [PostgreSQL and EF Core](#postgresql-and-ef-core)).
3. Configure User Secrets (OpenAI API key, JWT secret).
4. Apply EF Core migrations (only needed if using PostgreSQL audit logging).
5. Run the API.
6. Open Swagger and/or GraphQL Nitro.
7. Import the Postman collection and environment.

```bash
docker compose up -d
dotnet restore
dotnet ef database update --project ApiAggregator.Persistence --startup-project ApiAggregator.Api
dotnet run --project ApiAggregator.Api
```

URLs:

- Swagger: `http://localhost:5025/swagger`
- GraphQL Nitro: `https://localhost:7209/graphql/`

---

## REST API Endpoints

### Authenticate User

`POST /api/auth/login`

No authorization required.

Request:
```json
{
  "username": "demo",
  "password": "demo-password"
}
```

Response:
```json
{
  "success": true,
  "accessToken": "jwt-token-here",
  "expiresAt": "2026-06-15T12:00:00Z"
}
```

---

### Get Aggregated Data

`GET /api/aggregation`

Requires `Authorization: Bearer {accessToken}`.

Query parameters:

| Parameter | Description |
|---|---|
| `keyword` | Filters items whose title or description contains this text |
| `category` | Filters by exact category match |
| `source` | Filters by provider source (`GitHub`, `HackerNews`, `OpenMeteo`) |
| `fromDate` | Filters items published on or after this date |
| `toDate` | Filters items published on or before this date |
| `sortBy` | `date`, `relevance`, `category`, `source`, or `title` |
| `sortDirection` | `asc` (default) or `desc` |
| `providers` | Restrict to specific providers (repeatable query param) |

Example:
```text
GET /api/aggregation?keyword=dotnet&sortBy=date&sortDirection=desc
```

Response:
```json
{
  "items": [
    {
      "source": "GitHub",
      "title": "dotnet/runtime",
      "description": ".NET runtime repository",
      "category": "repository",
      "publishedAt": "2026-06-12T10:30:00Z",
      "relevanceScore": 98.5,
      "url": "https://github.com/dotnet/runtime"
    }
  ],
  "providers": [
    {
      "source": "GitHub",
      "success": true,
      "usedFallback": false,
      "responseTimeMs": 145,
      "errorMessage": null
    }
  ]
}
```

---

### Get API Statistics

`GET /api/statistics`

Requires `Authorization: Bearer {accessToken}`.

Response:
```json
{
  "apis": [
    {
      "apiName": "GitHub",
      "totalRequests": 3,
      "successfulRequests": 3,
      "failedRequests": 0,
      "averageResponseTimeMs": 345.33,
      "buckets": { "fast": 2, "average": 0, "slow": 1 }
    }
  ]
}
```

---

## GraphQL Operations

All GraphQL operations are available through Hot Chocolate Nitro at `https://localhost:7209/graphql/`, or via `POST` requests with a JSON body to the same URL.

### AuthenticateUser

```graphql
mutation AuthenticateUser($input: LoginInputTypeInput!) {
  login(input: $input) {
    success
    token
    expiresAt
  }
}
```

### AggregateData

```graphql
query AggregateData($input: AggregateDataInputTypeInput!) {
  aggregateData(input: $input) {
    items {
      source
      title
      description
      category
      publishedAt
      relevanceScore
      url
    }
    providers {
      source
      success
      usedFallback
      responseTimeMs
      errorMessage
    }
  }
}
```

### GetApiStatistics

```graphql
query GetApiStatistics {
  apiStatistics {
    apiName
    totalRequests
    successfulRequests
    failedRequests
    averageResponseTimeMs
    buckets {
      fast
      average
      slow
    }
  }
}
```

---

## Known Limitations

- The authentication implementation (single hardcoded demo user) is suitable for the assignment/demo and should be replaced with a production identity provider (e.g. Entra ID, Auth0, IdentityServer) in a real system.
- In-memory statistics reset whenever the application restarts.
- EF Core request logs are best-effort and intentionally do not block the main aggregation flow if PostgreSQL is unavailable.
- External API rate limits (particularly GitHub's unauthenticated search API) depend on the third-party providers and may cause occasional provider failures under heavy testing.
- Redis support is designed through the `IApiResponseCache` abstraction and a ready-to-activate `RedisApiResponseCache`, but local development uses the in-memory cache by default.
- The AI error enrichment path is rarely exercised in normal operation, since cache fallback is preferred when available; it was verified manually by temporarily pointing a provider at an invalid host.
