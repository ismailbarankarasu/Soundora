using Soundora.Application.Music.Models;

namespace Soundora.Application.Music.Abstractions;

public interface IMusicService
{
    Task<IReadOnlyList<LatestMusicDto>> GetLatestAsync(
        CancellationToken cancellationToken = default);
}