using Soundora.Application.Files.Abstractions;
using Soundora.Application.Files.Models;

namespace Soundora.Infrastructure.Files;

public class LocalAudioFileStorage : IAudioFileStorage
{
    private readonly AudioStorageSettings _settings;

    public LocalAudioFileStorage(AudioStorageSettings settings)
    {
        _settings = settings;
    }

    public async Task<StoredAudioFile> SaveAsync(
        Stream content,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(
                Path.GetExtension(originalFileName),
                ".mp3",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Yalnızca MP3 dosyası yükleyebilirsiniz.");
        }

        Directory.CreateDirectory(_settings.RootPath);

        var fileName = $"{Guid.NewGuid():N}.mp3";

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

                var buffer = new byte[81920];
                long totalBytes = 0;

                while (true)
                {
                    var bytesRead = await content.ReadAsync(
                        buffer.AsMemory(),
                        cancellationToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    totalBytes += bytesRead;

                    if (totalBytes > _settings.MaxFileSizeBytes)
                    {
                        throw new InvalidDataException(
                            "MP3 dosyası en fazla 20 MB olabilir.");
                    }

                    await destination.WriteAsync(
                        buffer.AsMemory(0, bytesRead),
                        cancellationToken);
                }

                if (totalBytes == 0)
                {
                    throw new InvalidDataException(
                        "Boş dosya yükleyemezsiniz.");
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            var durationInSeconds = ReadDurationInSeconds(fullPath);

            return new StoredAudioFile
            {
                FilePath = fileName,
                DurationInSeconds = durationInSeconds
            };
        }
        catch
        {
            if (fileCreated)
            {
                File.Delete(fullPath);
            }

            throw;
        }
    }

    public Task DeleteAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(filePath) ||
            filePath.Length != 36 ||
            !filePath.EndsWith(".mp3", StringComparison.Ordinal) ||
            !Guid.TryParseExact(filePath[..32], "N", out _))
        {
            throw new ArgumentException(
                "Geçersiz ses dosyası adı.",
                nameof(filePath));
        }

        var fullPath = Path.Combine(
            _settings.RootPath,
            filePath);

        File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private static int ReadDurationInSeconds(string fullPath)
    {
        try
        {
            using var audio = new TagLib.Mpeg.AudioFile(fullPath);

            var duration = audio.Properties.Duration.TotalSeconds;

            if (!double.IsFinite(duration) ||
                duration <= 0 ||
                duration > int.MaxValue ||
                audio.Properties.AudioBitrate <= 0 ||
                audio.Properties.AudioSampleRate <= 0)
            {
                throw new InvalidDataException(
                    "Dosyanın geçerli ses bilgileri okunamadı.");
            }

            return checked((int)Math.Ceiling(duration));
        }
        catch (TagLib.CorruptFileException exception)
        {
            throw new InvalidDataException(
                "Dosya bozuk veya desteklenen bir MP3 dosyası değil.",
                exception);
        }
        catch (TagLib.UnsupportedFormatException exception)
        {
            throw new InvalidDataException(
                "Dosyanın ses formatı desteklenmiyor.",
                exception);
        }
    }
}