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

    public async Task<UpdatePodcastRequest?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AudioContents
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                x.ContentType == ContentType.Podcast)
            .Select(x => new UpdatePodcastRequest
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CategoryId = x.CategoryId,
                ArtistId = x.ArtistId,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PodcastOperationResult> UpdateAsync(UpdatePodcastRequest request, CancellationToken cancellationToken = default)
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

        var podcast = await _context.AudioContents
            .FirstOrDefaultAsync(
                x => x.Id == request.Id &&
                     x.ContentType == ContentType.Podcast,
                cancellationToken);

        if (podcast is null)
        {
            return new PodcastOperationResult
            {
                Succeeded = false,
                Error = "Podcast bulunamadı."
            };
        }

        var categoryId = request.CategoryId!.Value;

        var categoryAllowed = await _context.Categories
            .AnyAsync(
                x => x.Id == categoryId &&
                     (x.IsActive || x.Id == podcast.CategoryId),
                cancellationToken);

        if (!categoryAllowed)
        {
            return new PodcastOperationResult
            {
                Succeeded = false,
                Error = "Seçilen kategori bulunamadı veya aktif değil."
            };
        }

        if (request.ArtistId.HasValue)
        {
            var artistId = request.ArtistId.Value;

            var hostAllowed = await _context.Artists
                .AnyAsync(
                    x => x.Id == artistId &&
                         (x.IsActive || x.Id == podcast.ArtistId),
                    cancellationToken);

            if (!hostAllowed)
            {
                return new PodcastOperationResult
                {
                    Succeeded = false,
                    Error = "Seçilen sunucu bulunamadı veya aktif değil."
                };
            }
        }

        podcast.Title = request.Title.Trim();

        podcast.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        podcast.CategoryId = categoryId;
        podcast.ArtistId = request.ArtistId;
        podcast.IsActive = request.IsActive;
        podcast.RequiredAccessLevel = AccessLevel.Gold;

        podcast.MarkAsUpdated();

        await _context.SaveChangesAsync(cancellationToken);

        return new PodcastOperationResult
        {
            Succeeded = true,
            Id = podcast.Id
        };
    }

    public async Task<DeletePodcastResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var podcast = await _context.AudioContents
            .FirstOrDefaultAsync(
                x => x.Id == id &&
                     x.ContentType == ContentType.Podcast,
                cancellationToken);

        if (podcast is null)
        {
            return new DeletePodcastResult
            {
                Succeeded = false,
                Error = "Podcast bulunamadı."
            };
        }

        var filePath = podcast.FilePath;

        _context.AudioContents.Remove(podcast);

        await _context.SaveChangesAsync(cancellationToken);

        return new DeletePodcastResult
        {
            Succeeded = true,
            FilePath = filePath
        };
    }

    public async Task<PodcastCatalogResult> GetCatalogAsync(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        const int pageSize = 6;

        search = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        if (search is not null && search.Length > 150)
        {
            search = search[..150];
        }

        var query = _context.AudioContents
            .AsNoTracking()
            .Where(x =>
                x.ContentType == ContentType.Podcast &&
                x.IsActive &&
                x.Category.IsActive);

        if (search is not null)
        {
            query = query.Where(x =>
                x.Title.Contains(search) ||
                (x.Artist != null && x.Artist.Name.Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling((double)totalCount / pageSize));

        page = Math.Clamp(page, 1, totalPages);

        var pagedQuery = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var items = await ProjectToDto(pagedQuery)
            .ToListAsync(cancellationToken);

        return new PodcastCatalogResult
        {
            Items = items,
            Search = search,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}