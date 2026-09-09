using Soundora.Application.Music.Models;

namespace Soundora.Application.Music.Abstractions;

public interface IMusicService
{
    Task<IReadOnlyList<LatestMusicDto>> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<MusicOperationResult> CreateAsync(CreateMusicRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdminMusicDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UpdateMusicRequest?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<MusicOperationResult> UpdateAsync(UpdateMusicRequest request, CancellationToken cancellationToken = default);
    Task<DeleteMusicResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}