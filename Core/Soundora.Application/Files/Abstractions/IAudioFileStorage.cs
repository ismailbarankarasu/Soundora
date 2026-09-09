using Soundora.Application.Files.Models;

namespace Soundora.Application.Files.Abstractions;

public interface IAudioFileStorage
{
    Task<StoredAudioFile> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<Stream?> OpenReadAsync(string filePath, CancellationToken cancellationToken = default);
}