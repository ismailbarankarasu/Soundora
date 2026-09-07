using Soundora.Domain.Common;
using Soundora.Domain.Enums;

namespace Soundora.Domain.Entities;

public class SubscriptionPackage : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationInDays { get; set; }

    public AccessLevel AccessLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<UserSubscription> UserSubscriptions { get; set; }
        = new List<UserSubscription>();
}