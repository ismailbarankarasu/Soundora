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

        builder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }
}