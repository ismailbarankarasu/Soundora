using Soundora.Domain.Common;

namespace Soundora.Domain.Entities;

public class UserSubscription : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid SubscriptionPackageId { get; set; }

    public SubscriptionPackage SubscriptionPackage { get; set; }
        = null!;

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsCurrentlyActive(DateTimeOffset currentTime)
    {
        return IsActive &&
               StartDate <= currentTime &&
               EndDate > currentTime;
    }
}