using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Podcasts.Abstractions;
using Soundora.Application.Podcasts.Models;
using Soundora.Domain.Entities;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class PodcastService : IPodcastService
{
    private readonly AppDbContext _context;

    public PodcastService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PodcastDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.AudioContents
            .AsNoTracking()
            .Where(x => x.ContentType == ContentType.Podcast)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        return await ProjectToDto(query)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PodcastDto>> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.AudioContents
            .AsNoTracking()
            .Where(x =>
                x.ContentType == ContentType.Podcast &&
                x.IsActive &&
                x.Category.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Take(6);

        return await ProjectToDto(query)
            .ToListAsync(cancellationToken);
    }

    public async Task<PodcastOperationResult> CreateAsync(CreatePodcastRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new PodcastOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        var categoryExists = await _context.Categories
            .AnyAsync(
                x => x.Id == request.CategoryId && x.IsActive,
                cancellationToken);

        if (!categoryExists)
        {
            return new PodcastOperationResult
            {
                Succeeded = false,
                Error = "Seçilen kategori bulunamadı veya aktif değil."
            };
        }

        if (request.ArtistId.HasValue)
        {
            var hostExists = await _context.Artists
                .AnyAsync(
                    x => x.Id == request.ArtistId.Value && x.IsActive,
                    cancellationToken);

            if (!hostExists)
            {
                return new PodcastOperationResult
                {
                    Succeeded = false,
                    Error = "Seçilen sunucu bulunamadı veya aktif değil."
                };
            }
        }

        var podcast = new AudioContent
        {
            Title = request.Title.Trim(),

            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),

            CategoryId = request.CategoryId,
            ArtistId = request.ArtistId,

            ContentType = ContentType.Podcast,
            RequiredAccessLevel = AccessLevel.Gold,

            FilePath = request.FilePath,

            CoverImagePath = string.IsNullOrWhiteSpace(request.CoverImagePath)
                ? null
                : request.CoverImagePath,

            DurationInSeconds = request.DurationInSeconds,
            IsActive = true
        };

        _context.AudioContents.Add(podcast);

        await _context.SaveChangesAsync(cancellationToken);

        return new PodcastOperationResult
        {
            Succeeded = true,
            Id = podcast.Id
        };
    }

    private static IQueryable<PodcastDto> ProjectToDto(IQueryable<AudioContent> query)
    {
        return query.Select(x => new PodcastDto
        {
            Id = x.Id,
            Title = x.Title,

            HostName = x.Artist != null
                ? x.Artist.Name
                : "Belirtilmedi",

            CategoryName = x.Category.Name,
            CoverImageUrl = x.CoverImagePath,
            DurationInSeconds = x.DurationInSeconds,
            IsActive = x.IsActive
        });
    }
}