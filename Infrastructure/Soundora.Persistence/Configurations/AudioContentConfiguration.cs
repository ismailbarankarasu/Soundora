using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soundora.Domain.Entities;

namespace Soundora.Persistence.Configurations;

public sealed class AudioContentConfiguration
    : IEntityTypeConfiguration<AudioContent>
{
    public void Configure(EntityTypeBuilder<AudioContent> builder)
    {
        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CoverImagePath)
            .HasMaxLength(500);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.AudioContents)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Artist)
            .WithMany(x => x.AudioContents)
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.ArtistId);

        builder.HasIndex(x => new
        {
            x.ContentType,
            x.IsActive
        });
    }
}