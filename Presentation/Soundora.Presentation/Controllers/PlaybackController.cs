using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Authentication.Constants;
using Soundora.Application.Authentication.Models;
using Soundora.Application.Files.Abstractions;
using Soundora.Application.Playback.Abstractions;
using Soundora.Application.Playback.Models;
using Soundora.Domain.Enums;
using System.Globalization;

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
        if (!TryReadSubscription(out var tokenSubscription))
        {
            return Unauthorized(new
            {
                message = "Token paket bilgileri geçersiz. Dinle butonuna tekrar basınız."
            });
        }

        var access = await _playbackAccessService.CheckAsync(
            userId,
            id,
            tokenSubscription,
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

            case PlaybackAccessStatus.TokenOutdated:
                return Unauthorized(new
                {
                    message = access.Error
                });

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
    private bool TryReadSubscription(out JwtSubscriptionInfo? subscription)
    {
        subscription = null;

        var levels = User.FindAll(JwtClaimNames.PackageLevel).ToArray();
        var subscriptionIds = User.FindAll(JwtClaimNames.SubscriptionId).ToArray();
        var packageIds = User.FindAll(JwtClaimNames.PackageId).ToArray();
        var expirations = User.FindAll(JwtClaimNames.PackageExpiresAt).ToArray();

        if (levels.Length != 1)
        {
            return false;
        }

        if (levels[0].Value == "0")
        {
            return subscriptionIds.Length == 0 &&
                   packageIds.Length == 0 &&
                   expirations.Length == 0;
        }

        if (levels[0].Value != "1" && levels[0].Value != "2")
        {
            return false;
        }

        if (subscriptionIds.Length != 1 ||
            packageIds.Length != 1 ||
            expirations.Length != 1)
        {
            return false;
        }

        if (!Guid.TryParse(subscriptionIds[0].Value, out var subscriptionId) ||
            subscriptionId == Guid.Empty ||
            !Guid.TryParse(packageIds[0].Value, out var packageId) ||
            packageId == Guid.Empty)
        {
            return false;
        }

        if (!long.TryParse(
                expirations[0].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var expiresAtUnix) ||
            expiresAtUnix < 0 ||
            expiresAtUnix > 253402300799L)
        {
            return false;
        }

        subscription = new JwtSubscriptionInfo
        {
            SubscriptionId = subscriptionId,
            PackageId = packageId,

            AccessLevel = levels[0].Value == "2"
                ? AccessLevel.Gold
                : AccessLevel.Basic,

            ExpiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix)
        };

        return true;
    }
}