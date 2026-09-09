using Soundora.Application.Authentication.Models;
using Soundora.Application.Playback.Models;

namespace Soundora.Application.Playback.Abstractions;

public interface IPlaybackAccessService
{
    Task<PlaybackAccessResult> CheckAsync(Guid userId, Guid contentId, JwtSubscriptionInfo? tokenSubscription, CancellationToken cancellationToken = default);
}