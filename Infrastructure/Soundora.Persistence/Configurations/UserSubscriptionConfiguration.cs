using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Soundora.Domain.Entities;
using Soundora.Persistence.Identity;

namespace Soundora.Persistence.Configurations;

public sealed class UserSubscriptionConfiguration
    : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(
        EntityTypeBuilder<UserSubscription> builder)
    {
        builder.HasOne(x => x.SubscriptionPackage)
            .WithMany(x => x.UserSubscriptions)
            .HasForeignKey(x => x.SubscriptionPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<AppUser>()
            .WithMany(x => x.UserSubscriptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.IsActive,
            x.EndDate
        });

        builder.HasIndex(x => x.SubscriptionPackageId);
    }
}