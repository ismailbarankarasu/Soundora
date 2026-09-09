namespace Soundora.Application.Podcasts.Models;

public class PodcastOperationResult
{
    public bool Succeeded { get; init; }

    public Guid? Id { get; init; }

    public string? Error { get; init; }
}