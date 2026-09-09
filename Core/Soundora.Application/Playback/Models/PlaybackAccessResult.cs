namespace Soundora.Application.Playback.Models;

public class PlaybackAccessResult
{
    public PlaybackAccessStatus Status { get; init; }

    public string? Error { get; init; }

    public string? FilePath { get; init; }
}