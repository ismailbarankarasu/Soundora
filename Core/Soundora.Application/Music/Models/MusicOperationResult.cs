namespace Soundora.Application.Music.Models;

public class MusicOperationResult
{
    public bool Succeeded { get; init; }

    public Guid? Id { get; init; }

    public string? Error { get; init; }
    public string? PreviousCoverImagePath { get; init; }
}