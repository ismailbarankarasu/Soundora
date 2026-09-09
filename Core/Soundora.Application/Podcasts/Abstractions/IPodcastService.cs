using Soundora.Application.Podcasts.Models;

namespace Soundora.Application.Podcasts.Abstractions;

public interface IPodcastService
{
    Task<IReadOnlyList<PodcastDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PodcastDto>> GetLatestAsync(CancellationToken cancellationToken = default);

    Task<PodcastOperationResult> CreateAsync(CreatePodcastRequest request, CancellationToken cancellationToken = default);
}