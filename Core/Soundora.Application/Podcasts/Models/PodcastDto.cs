namespace Soundora.Application.Podcasts.Models;

public class PodcastDto
{
    public Guid Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string HostName { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string? CoverImageUrl { get; init; }

    public int DurationInSeconds { get; init; }

    public bool IsActive { get; init; }
}