using Soundora.Domain.Enums;

namespace Soundora.Application.Authentication.Models;

public class JwtSubscriptionInfo
{
    public Guid SubscriptionId { get; init; }

    public Guid PackageId { get; init; }

    public AccessLevel AccessLevel { get; init; }

    public DateTimeOffset ExpiresAtUtc { get; init; }
}