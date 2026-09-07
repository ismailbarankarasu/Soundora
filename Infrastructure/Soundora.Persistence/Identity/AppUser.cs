using Microsoft.AspNetCore.Identity;
using Soundora.Domain.Entities;

namespace Soundora.Persistence.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
        = DateTimeOffset.UtcNow;

    public ICollection<UserSubscription> UserSubscriptions { get; set; }
        = new List<UserSubscription>();
}