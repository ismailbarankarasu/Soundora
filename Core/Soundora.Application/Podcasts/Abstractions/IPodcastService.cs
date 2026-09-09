using Soundora.Application.Podcasts.Models;

namespace Soundora.Application.Podcasts.Abstractions;

public interface IPodcastService
{
    Task<IReadOnlyList<PodcastDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PodcastDto>> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<PodcastOperationResult> CreateAsync(CreatePodcastRequest request, CancellationToken cancellationToken = default);
    Task<UpdatePodcastRequest?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PodcastOperationResult> UpdateAsync(UpdatePodcastRequest request, CancellationToken cancellationToken = default);
    Task<DeletePodcastResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}