using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiAggregator.Persistence;

public sealed class ApiAggregatorDbContext : DbContext
{
    public DbSet<ExternalApiProvider> ExternalApiProviders => Set<ExternalApiProvider>();
    public DbSet<AggregatedItem> AggregatedItems => Set<AggregatedItem>();
    public DbSet<ApiRequestLog> ApiRequestLogs => Set<ApiRequestLog>();
    public DbSet<ApiPerformanceSnapshot> ApiPerformanceSnapshots => Set<ApiPerformanceSnapshot>();

    public ApiAggregatorDbContext(DbContextOptions<ApiAggregatorDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApiAggregatorDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}