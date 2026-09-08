namespace Soundora.Application.Files.Models;

public class StoredAudioFile
{
    public string FilePath { get; init; } = string.Empty;
    public int DurationInSeconds { get; init; }
}