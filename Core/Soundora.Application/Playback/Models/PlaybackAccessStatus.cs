namespace Soundora.Application.Playback.Models;

public enum PlaybackAccessStatus
{
    Allowed = 1,
    UserNotFound = 2,
    ContentNotFound = 3,
    SubscriptionRequired = 4,
    UpgradeRequired = 5,
    TokenOutdated = 6
}