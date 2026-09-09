namespace Soundora.Application.Music.Models;

public class AdminMusicDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string ArtistName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string AccessLevelName { get; init; } = string.Empty;

    public int DurationInSeconds { get; init; }

    public bool IsActive { get; init; }
}