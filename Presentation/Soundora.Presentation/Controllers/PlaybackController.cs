using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Files.Abstractions;
using Soundora.Application.Playback.Abstractions;
using Soundora.Application.Playback.Models;

namespace Soundora.Presentation.Controllers;

[Authorize(Policy = "PlaybackJwt")]
[Route("playback")]
public class PlaybackController : Controller
{
    private readonly IPlaybackAccessService _playbackAccessService;
    private readonly IAudioFileStorage _audioFileStorage;

    public PlaybackController(IPlaybackAccessService playbackAccessService, IAudioFileStorage audioFileStorage)
    {
        _playbackAccessService = playbackAccessService;
        _audioFileStorage = audioFileStorage;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Play(
        Guid id,
        CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "no-store";

        var userIdClaim = User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId) ||
            userId == Guid.Empty)
        {
            return Unauthorized(new
            {
                message = "Geçerli bir kullanıcı kimliği bulunamadı."
            });
        }

        var access = await _playbackAccessService.CheckAsync(
            userId,
            id,
            cancellationToken);

        switch (access.Status)
        {
            case PlaybackAccessStatus.UserNotFound:
                return Unauthorized(new
                {
                    message = access.Error
                });

            case PlaybackAccessStatus.ContentNotFound:
                return NotFound(new
                {
                    message = access.Error
                });

            case PlaybackAccessStatus.SubscriptionRequired:
            case PlaybackAccessStatus.UpgradeRequired:
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = access.Error
                    });

            case PlaybackAccessStatus.Allowed:
                break;

            default:
                return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (string.IsNullOrWhiteSpace(access.FilePath))
        {
            return NotFound(new
            {
                message = "Ses dosyası bulunamadı."
            });
        }

        var stream = await _audioFileStorage.OpenReadAsync(
            access.FilePath,
            cancellationToken);

        if (stream is null)
        {
            return NotFound(new
            {
                message = "Ses dosyası bulunamadı."
            });
        }

        return File(
            stream,
            "audio/mpeg",
            enableRangeProcessing: true);
    }
}