namespace Soundora.Application.Podcasts.Models;

public class PodcastCatalogResult
{
    public IReadOnlyList<PodcastDto> Items { get; init; }
        = Array.Empty<PodcastDto>();

    public string? Search { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 6;

    public int TotalCount { get; init; }

    public int TotalPages => PageSize > 0
        ? (int)Math.Ceiling((double)TotalCount / PageSize)
        : 0;
}