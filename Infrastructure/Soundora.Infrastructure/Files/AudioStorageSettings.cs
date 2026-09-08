namespace Soundora.Infrastructure.Files;

public class AudioStorageSettings
{
    public string RootPath { get; init; } = string.Empty;

    public long MaxFileSizeBytes { get; init; }
        = 20 * 1024 * 1024;
}