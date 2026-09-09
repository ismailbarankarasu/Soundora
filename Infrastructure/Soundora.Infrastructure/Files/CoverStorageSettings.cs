namespace Soundora.Infrastructure.Files;

public class CoverStorageSettings
{
    public string RootPath { get; init; } = string.Empty;

    public string RequestPath { get; init; } = "/uploads/covers";

    public long MaxFileSizeBytes { get; init; }
        = 5 * 1024 * 1024;
}