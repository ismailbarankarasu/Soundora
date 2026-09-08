namespace Soundora.Application.Music.Models;

public class LatestMusicDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ArtistName { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public string AccessLevelName { get; set; } = string.Empty;
}