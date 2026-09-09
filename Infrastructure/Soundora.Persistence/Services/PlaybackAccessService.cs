using Microsoft.EntityFrameworkCore;
using Soundora.Application.Authentication.Models;
using Soundora.Application.Playback.Abstractions;
using Soundora.Application.Playback.Models;
using Soundora.Application.Subscriptions.Abstractions;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class PlaybackAccessService : IPlaybackAccessService
{
    private readonly AppDbContext _context;
    private readonly ISubscriptionService _subscriptionService;

    public PlaybackAccessService(AppDbContext context, ISubscriptionService subscriptionService)
    {
        _context = context;
        _subscriptionService = subscriptionService;
    }

    public async Task<PlaybackAccessResult> CheckAsync(Guid userId, Guid contentId, JwtSubscriptionInfo? tokenSubscription, CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.Id == userId, cancellationToken);

        if (!userExists)
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.UserNotFound,
                Error = "Kullanıcı bulunamadı."
            };
        }

        var content = await _context.AudioContents
            .AsNoTracking()
            .Where(x =>
                x.Id == contentId &&
                x.IsActive &&
                x.Category.IsActive &&
                (x.ContentType == ContentType.Music ||
                 x.ContentType == ContentType.Podcast))
            .Select(x => new
            {
                x.FilePath,
                x.RequiredAccessLevel
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (content is null ||
            string.IsNullOrWhiteSpace(content.FilePath) ||
            (content.RequiredAccessLevel != AccessLevel.Basic &&
             content.RequiredAccessLevel != AccessLevel.Gold))
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.ContentNotFound,
                Error = "İçerik bulunamadı veya oynatmaya açık değil."
            };
        }

        var currentSubscription =
            await _subscriptionService.GetActiveForTokenAsync(
                userId,
                cancellationToken);

        if (currentSubscription is null)
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.SubscriptionRequired,
                Error = "Bu içeriği dinlemek için aktif bir aboneliğiniz olmalıdır."
            };
        }

        if (tokenSubscription is null ||
            tokenSubscription.SubscriptionId != currentSubscription.SubscriptionId ||
            tokenSubscription.PackageId != currentSubscription.PackageId ||
            tokenSubscription.AccessLevel != currentSubscription.AccessLevel ||
            tokenSubscription.ExpiresAtUtc.ToUnixTimeSeconds() !=
                currentSubscription.ExpiresAtUtc.ToUnixTimeSeconds())
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.TokenOutdated,
                Error = "Paket bilgileriniz değişmiş. Dinle butonuna tekrar basınız."
            };
        }

        if (tokenSubscription.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.SubscriptionRequired,
                Error = "Aboneliğinizin süresi dolmuş."
            };
        }

        var hasAccess =
            tokenSubscription.AccessLevel == AccessLevel.Gold ||
            (tokenSubscription.AccessLevel == AccessLevel.Basic &&
             content.RequiredAccessLevel == AccessLevel.Basic);

        if (!hasAccess)
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.UpgradeRequired,
                Error = "Bu içeriği dinlemek için Gold paketine geçmelisiniz."
            };
        }

        return new PlaybackAccessResult
        {
            Status = PlaybackAccessStatus.Allowed,
            FilePath = content.FilePath
        };
    }
}