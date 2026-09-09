using Soundora.Application.Music.Models;

namespace Soundora.Application.Music.Abstractions;

public interface IMusicService
{
    Task<IReadOnlyList<LatestMusicDto>> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<MusicOperationResult> CreateAsync(CreateMusicRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminMusicDto>> GetAllAsync(CancellationToken cancellationToken = default);
}