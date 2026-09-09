using Soundora.Application.Files.Models;

namespace Soundora.Application.Files.Abstractions;

public interface ICoverFileStorage
{
    Task<StoredCoverFile> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string filePath, CancellationToken cancellationToken = default);
}