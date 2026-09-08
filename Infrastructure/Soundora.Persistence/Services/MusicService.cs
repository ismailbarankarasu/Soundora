using Microsoft.EntityFrameworkCore;
using Soundora.Application.Music.Abstractions;
using Soundora.Application.Music.Models;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class MusicService : IMusicService
{
    private readonly AppDbContext _context;

    public MusicService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LatestMusicDto>> GetLatestAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.AudioContents
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.ContentType == ContentType.Music)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(6)
            .Select(x => new LatestMusicDto
            {
                Id = x.Id,
                Title = x.Title,
                ArtistName = x.Artist != null
                    ? x.Artist.Name
                    : "Bilinmeyen Sanatçı",
                CoverImageUrl = x.CoverImagePath,
                AccessLevelName =
                    x.RequiredAccessLevel == AccessLevel.Gold
                        ? "Gold"
                        : x.RequiredAccessLevel == AccessLevel.Basic
                            ? "Basic"
                            : "Standart"
            })
            .ToListAsync(cancellationToken);
    }
}