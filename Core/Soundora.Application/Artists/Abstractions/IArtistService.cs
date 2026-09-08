using Soundora.Application.Artists.Models;

namespace Soundora.Application.Artists.Abstractions;

public interface IArtistService
{
    Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ArtistOperationResult> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default);

    Task<ArtistDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ArtistOperationResult> UpdateAsync(UpdateArtistRequest request, CancellationToken cancellationToken = default);

    Task<ArtistOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}