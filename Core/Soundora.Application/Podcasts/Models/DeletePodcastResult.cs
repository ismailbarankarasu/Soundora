namespace Soundora.Application.Podcasts.Models;

public class DeletePodcastResult
{
    public bool Succeeded { get; init; }

    public string? Error { get; init; }

    public string? FilePath { get; init; }
    public string? CoverImagePath { get; init; }
}