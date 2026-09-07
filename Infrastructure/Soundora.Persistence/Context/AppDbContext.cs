using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Soundora.Domain.Entities;
using Soundora.Persistence.Identity;

namespace Soundora.Persistence.Contexts;

public class AppDbContext
    : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories =>
        Set<Category>();

    public DbSet<Artist> Artists =>
        Set<Artist>();

    public DbSet<AudioContent> AudioContents =>
        Set<AudioContent>();

    public DbSet<SubscriptionPackage> SubscriptionPackages =>
        Set<SubscriptionPackage>();

    public DbSet<UserSubscription> UserSubscriptions =>
        Set<UserSubscription>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SubscriptionPackage>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Entity<AudioContent>()
            .HasOne(x => x.Category)
            .WithMany(x => x.AudioContents)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AudioContent>()
            .HasOne(x => x.Artist)
            .WithMany(x => x.AudioContents)
            .HasForeignKey(x => x.ArtistId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<UserSubscription>()
            .HasOne(x => x.SubscriptionPackage)
            .WithMany(x => x.UserSubscriptions)
            .HasForeignKey(x => x.SubscriptionPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasMany(x => x.UserSubscriptions)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}