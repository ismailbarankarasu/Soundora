using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Soundora.Application.Files.Abstractions;
using Soundora.Application.Files.Models;

namespace Soundora.Infrastructure.Files;

public class LocalCoverFileStorage : ICoverFileStorage
{
    private readonly CoverStorageSettings _settings;

    public LocalCoverFileStorage(CoverStorageSettings settings)
    {
        _settings = settings;
    }

    public async Task<StoredCoverFile> SaveAsync(Stream content, string originalFileName, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (extension is not ".jpg" and not ".jpeg" and not ".png")
        {
            throw new InvalidDataException(
                "Kapak görseli JPG veya PNG olmalıdır.");
        }

        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        long totalBytes = 0;

        while (true)
        {
            var bytesRead = await content.ReadAsync(
                chunk.AsMemory(),
                cancellationToken);

            if (bytesRead == 0)
            {
                break;
            }

            totalBytes += bytesRead;

            if (totalBytes > _settings.MaxFileSizeBytes)
            {
                throw new InvalidDataException(
                    "Kapak görseli en fazla 5 MB olabilir.");
            }

            await buffer.WriteAsync(
                chunk.AsMemory(0, bytesRead),
                cancellationToken);
        }

        if (totalBytes == 0)
        {
            throw new InvalidDataException(
                "Boş görsel yükleyemezsiniz.");
        }

        try
        {
            buffer.Position = 0;

            var format = await Image.DetectFormatAsync(
                buffer,
                cancellationToken);

            var expectedFormat = extension == ".png"
                ? "PNG"
                : "JPEG";

            if (!string.Equals(
                    format.Name,
                    expectedFormat,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    "Dosyanın içeriği JPG/PNG uzantısıyla eşleşmiyor.");
            }

            buffer.Position = 0;

            var info = await Image.IdentifyAsync(
                buffer,
                cancellationToken);

            if (info.Width <= 0 ||
                info.Height <= 0 ||
                info.Width > 4096 ||
                info.Height > 4096 ||
                (long)info.Width * info.Height > 16_000_000)
            {
                throw new InvalidDataException(
                    "Görsel en fazla 4096 × 4096 piksel ve toplam 16 milyon piksel olabilir.");
            }

            buffer.Position = 0;

            using var image = await Image.LoadAsync(
                new DecoderOptions
                {
                    MaxFrames = 1
                },
                buffer,
                cancellationToken);

            image.Mutate(context =>
            {
                context.AutoOrient();

                if (image.Width > 1000 || image.Height > 1000)
                {
                    context.Resize(new ResizeOptions
                    {
                        Size = new Size(1000, 1000),
                        Mode = ResizeMode.Max
                    });
                }
            });

            Directory.CreateDirectory(_settings.RootPath);

            var fileName = $"{Guid.NewGuid():N}.png";
            var fullPath = Path.Combine(
                _settings.RootPath,
                fileName);

            var fileCreated = false;

            try
            {
                await using (var destination = new FileStream(
                    fullPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true))
                {
                    fileCreated = true;

                    await image.SaveAsync(
                        destination,
                        new PngEncoder
                        {
                            SkipMetadata = true
                        },
                        cancellationToken);
                }
            }
            catch
            {
                if (fileCreated)
                {
                    File.Delete(fullPath);
                }

                throw;
            }

            return new StoredCoverFile
            {
                FilePath =
                    $"~{_settings.RequestPath.TrimEnd('/')}/{fileName}"
            };
        }
        catch (UnknownImageFormatException exception)
        {
            throw new InvalidDataException(
                "Dosya desteklenen bir görsel değil.",
                exception);
        }
        catch (InvalidImageContentException exception)
        {
            throw new InvalidDataException(
                "Görsel bozuk veya okunamıyor.",
                exception);
        }
    }

    public Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var prefix = $"~{_settings.RequestPath.TrimEnd('/')}/";

        if (string.IsNullOrWhiteSpace(filePath) ||
            !filePath.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Geçersiz kapak dosyası yolu.",
                nameof(filePath));
        }

        var fileName = filePath[prefix.Length..];

        if (fileName.Length != 36 ||
            !fileName.EndsWith(".png", StringComparison.Ordinal) ||
            !Guid.TryParseExact(fileName[..32], "N", out _))
        {
            throw new ArgumentException(
                "Geçersiz kapak dosyası adı.",
                nameof(filePath));
        }

        var fullPath = Path.Combine(
            _settings.RootPath,
            fileName);

        File.Delete(fullPath);

        return Task.CompletedTask;
    }
}