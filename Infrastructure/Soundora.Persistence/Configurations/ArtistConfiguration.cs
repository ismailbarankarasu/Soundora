using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soundora.Domain.Entities;

namespace Soundora.Persistence.Configurations;

public sealed class ArtistConfiguration
    : IEntityTypeConfiguration<Artist>
{
    public void Configure(EntityTypeBuilder<Artist> builder)
    {
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Biography)
            .HasMaxLength(1000);

        builder.Property(x => x.ImagePath)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name);
    }
}