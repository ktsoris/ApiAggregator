using ApiAggregator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiAggregator.Persistence.Configurations;

public sealed class AggregatedItemConfiguration : IEntityTypeConfiguration<AggregatedItem>
{
    public void Configure(EntityTypeBuilder<AggregatedItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Category)
            .HasMaxLength(100);

        builder.Property(x => x.Url)
            .HasMaxLength(1000);

        builder.Property(x => x.RetrievedAt)
            .IsRequired();
    }
}