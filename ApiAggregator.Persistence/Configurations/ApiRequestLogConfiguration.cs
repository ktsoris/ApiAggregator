using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiAggregator.Persistence.Configurations;

public sealed class ApiRequestLogConfiguration : IEntityTypeConfiguration<ApiRequestLog>
{
    public void Configure(EntityTypeBuilder<ApiRequestLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApiName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Success)
            .IsRequired();

        builder.Property(x => x.UsedFallback)
            .IsRequired();

        builder.Property(x => x.ResponseTimeMs)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.RequestedAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(x => x.ApiName);
        builder.HasIndex(x => x.RequestedAt);
    }
}