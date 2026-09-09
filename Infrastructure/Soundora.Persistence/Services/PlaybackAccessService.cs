using Microsoft.EntityFrameworkCore;
using Soundora.Application.Playback.Abstractions;
using Soundora.Application.Playback.Models;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class PlaybackAccessService : IPlaybackAccessService
{
    private readonly AppDbContext _context;

    public PlaybackAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlaybackAccessResult> CheckAsync(Guid userId, Guid contentId, CancellationToken cancellationToken = default)
    {
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == userId,
                cancellationToken);

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

        var now = DateTimeOffset.UtcNow;

        var activeLevels = await _context.UserSubscriptions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                x.StartDate <= now &&
                x.EndDate > now &&
                x.SubscriptionPackage.IsActive &&
                (x.SubscriptionPackage.AccessLevel == AccessLevel.Basic ||
                 x.SubscriptionPackage.AccessLevel == AccessLevel.Gold))
            .Select(x => x.SubscriptionPackage.AccessLevel)
            .ToListAsync(cancellationToken);

        if (activeLevels.Count == 0)
        {
            return new PlaybackAccessResult
            {
                Status = PlaybackAccessStatus.SubscriptionRequired,
                Error = "Bu içeriği dinlemek için aktif bir aboneliğiniz olmalıdır."
            };
        }

        var hasAccess = activeLevels.Any(level =>
            level == AccessLevel.Gold ||
            (level == AccessLevel.Basic &&
             content.RequiredAccessLevel == AccessLevel.Basic));

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