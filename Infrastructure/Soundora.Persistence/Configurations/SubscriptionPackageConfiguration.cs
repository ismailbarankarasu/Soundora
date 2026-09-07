using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soundora.Domain.Entities;

namespace Soundora.Persistence.Configurations;

public sealed class SubscriptionPackageConfiguration
    : IEntityTypeConfiguration<SubscriptionPackage>
{
    public void Configure(
        EntityTypeBuilder<SubscriptionPackage> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}