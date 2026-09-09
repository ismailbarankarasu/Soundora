namespace Soundora.Application.Music.Models;

public class DeleteMusicResult
{
    public bool Succeeded { get; init; }

    public string? Error { get; init; }

    public string? FilePath { get; init; }
    public string? CoverImagePath { get; init; }
}