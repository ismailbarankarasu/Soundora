namespace Soundora.Application.Music.Models;

public class MusicCatalogResult
{
    public IReadOnlyList<LatestMusicDto> Items { get; init; }
        = Array.Empty<LatestMusicDto>();

    public string? Search { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);
}