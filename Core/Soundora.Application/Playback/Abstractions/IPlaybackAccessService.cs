using Soundora.Application.Playback.Models;

namespace Soundora.Application.Playback.Abstractions;

public interface IPlaybackAccessService
{
    Task<PlaybackAccessResult> CheckAsync(Guid userId, Guid contentId, CancellationToken cancellationToken = default);
}