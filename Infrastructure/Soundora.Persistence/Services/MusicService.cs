using Microsoft.EntityFrameworkCore;
using Soundora.Application.Music.Abstractions;
using Soundora.Application.Music.Models;
using Soundora.Domain.Entities;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;
using System.ComponentModel.DataAnnotations;

namespace Soundora.Persistence.Services;

public class MusicService : IMusicService
{
    private readonly AppDbContext _context;

    public MusicService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MusicOperationResult> CreateAsync(
    CreateMusicRequest request,
    CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new MusicOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        if (request.RequiredAccessLevel != AccessLevel.Basic &&
            request.RequiredAccessLevel != AccessLevel.Gold)
        {
            return new MusicOperationResult
            {
                Succeeded = false,
                Error = "Basic veya Gold paket seviyesini seçiniz."
            };
        }

        var categoryExists = await _context.Categories.AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);

        if (!categoryExists)
        {
            return new MusicOperationResult
            {
                Succeeded = false,
                Error = "Seçilen kategori bulunamadı veya aktif değil."
            };
        }

        if (request.ArtistId.HasValue)
        {
            var artistExists = await _context.Artists
                .AnyAsync(
                    x => x.Id == request.ArtistId.Value && x.IsActive,
                    cancellationToken);

            if (!artistExists)
            {
                return new MusicOperationResult
                {
                    Succeeded = false,
                    Error = "Seçilen sanatçı bulunamadı veya aktif değil."
                };
            }
        }

        var music = new AudioContent
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),

            CategoryId = request.CategoryId,
            ArtistId = request.ArtistId,

            ContentType = ContentType.Music,
            RequiredAccessLevel = request.RequiredAccessLevel,
            DurationInSeconds = request.DurationInSeconds,
            FilePath = request.FilePath,

            CoverImagePath = string.IsNullOrWhiteSpace(request.CoverImagePath)
                ? null
                : request.CoverImagePath,

            IsActive = true
        };

        _context.AudioContents.Add(music);

        await _context.SaveChangesAsync(cancellationToken);

        return new MusicOperationResult
        {
            Succeeded = true,
            Id = music.Id
        };
    }

    public async Task<IReadOnlyList<AdminMusicDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AudioContents
            .AsNoTracking()
            .Where(x => x.ContentType == ContentType.Music)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new AdminMusicDto
            {
                Id = x.Id,
                Title = x.Title,

                ArtistName = x.Artist != null
                    ? x.Artist.Name
                    : "Belirtilmedi",

                CategoryName = x.Category.Name,

                AccessLevelName =
                    x.RequiredAccessLevel == AccessLevel.Gold
                        ? "Gold"
                        : x.RequiredAccessLevel == AccessLevel.Basic
                            ? "Basic"
                            : "Standart",

                DurationInSeconds = x.DurationInSeconds,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
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