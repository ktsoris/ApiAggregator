using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiAggregator.Persistence.Configurations;

public sealed class ApiPerformanceSnapshotConfiguration : IEntityTypeConfiguration<ApiPerformanceSnapshot>
{
    public void Configure(EntityTypeBuilder<ApiPerformanceSnapshot> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApiName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AverageResponseTimeMs)
            .IsRequired();

        builder.Property(x => x.TotalRequests)
            .IsRequired();

        builder.Property(x => x.SuccessfulRequests)
            .IsRequired();

        builder.Property(x => x.FailedRequests)
            .IsRequired();

        builder.Property(x => x.SnapshotTakenAt)
            .IsRequired();

        builder.HasIndex(x => x.ApiName);
        builder.HasIndex(x => x.SnapshotTakenAt);
    }
}