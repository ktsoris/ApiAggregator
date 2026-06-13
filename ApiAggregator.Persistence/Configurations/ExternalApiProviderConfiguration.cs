using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiAggregator.Persistence.Configurations;

public sealed class ExternalApiProviderConfiguration : IEntityTypeConfiguration<ExternalApiProvider>
{
    public void Configure(EntityTypeBuilder<ExternalApiProvider> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.BaseUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();
    }
}