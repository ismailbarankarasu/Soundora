using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Artists.Abstractions;
using Soundora.Application.Artists.Models;
using Soundora.Domain.Entities;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class ArtistService : IArtistService
{
    private readonly AppDbContext _context;

    public ArtistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Artists
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ArtistDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ArtistOperationResult> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.Artists
            .AnyAsync(
                x => x.Name == name,
                cancellationToken);

        if (exists)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = "Bu isimde bir sanatçı zaten mevcut."
            };
        }

        var artist = new Artist
        {
            Name = name,
            IsActive = true
        };

        _context.Artists.Add(artist);

        await _context.SaveChangesAsync(cancellationToken);

        return new ArtistOperationResult
        {
            Succeeded = true
        };
    }

    public async Task<ArtistDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Artists
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ArtistDto
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ArtistOperationResult> UpdateAsync(UpdateArtistRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        var artist = await _context.Artists
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (artist is null)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = "Sanatçı bulunamadı."
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.Artists
            .AnyAsync(
                x => x.Name == name && x.Id != request.Id,
                cancellationToken);

        if (exists)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = "Bu isimde başka bir sanatçı mevcut."
            };
        }

        artist.Name = name;
        artist.IsActive = request.IsActive;
        artist.MarkAsUpdated();

        await _context.SaveChangesAsync(cancellationToken);

        return new ArtistOperationResult
        {
            Succeeded = true
        };
    }

    public async Task<ArtistOperationResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var artist = await _context.Artists
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (artist is null)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = "Sanatçı bulunamadı."
            };
        }

        var hasContent = await _context.AudioContents
            .AnyAsync(
                x => x.ArtistId == id,
                cancellationToken);

        if (hasContent)
        {
            return new ArtistOperationResult
            {
                Succeeded = false,
                Error = "Bu sanatçıya bağlı müzik veya podcast bulunduğu için silinemez. Sanatçıyı pasif yapabilirsiniz."
            };
        }

        _context.Artists.Remove(artist);

        await _context.SaveChangesAsync(cancellationToken);

        return new ArtistOperationResult
        {
            Succeeded = true
        };
    }
}