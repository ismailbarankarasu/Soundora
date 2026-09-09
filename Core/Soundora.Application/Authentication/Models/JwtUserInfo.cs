namespace Soundora.Application.Authentication.Models;

public class JwtUserInfo
{
    public Guid UserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; init; }
        = Array.Empty<string>();

    public JwtSubscriptionInfo? Subscription { get; init; }
}