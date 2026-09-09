using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Packages.Abstractions;
using Soundora.Application.Packages.Models;
using Soundora.Domain.Entities;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class PackageService : IPackageService
{
    private readonly AppDbContext _context;

    public PackageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPackages
            .AsNoTracking()
            .OrderBy(x => x.AccessLevel)
            .ThenBy(x => x.DurationInDays)
            .ThenBy(x => x.Name)
            .Select(x => new PackageDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DurationInDays = x.DurationInDays,
                AccessLevel = x.AccessLevel,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PackageOperationResult> CreateAsync(CreatePackageRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        if (decimal.Round(request.Price, 2) != request.Price)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = "Fiyat en fazla iki ondalık basamak içerebilir."
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.SubscriptionPackages
            .AnyAsync(
                x => x.Name == name,
                cancellationToken);

        if (exists)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = "Bu isimde bir paket zaten mevcut."
            };
        }

        var package = new SubscriptionPackage
        {
            Name = name,

            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),

            Price = request.Price,
            DurationInDays = request.DurationInDays,
            AccessLevel = request.AccessLevel,
            IsActive = true
        };

        _context.SubscriptionPackages.Add(package);

        await _context.SaveChangesAsync(cancellationToken);

        return new PackageOperationResult
        {
            Succeeded = true
        };
    }

    public async Task<UpdatePackageRequest?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPackages
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UpdatePackageRequest
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                DurationInDays = x.DurationInDays,
                AccessLevel = x.AccessLevel,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PackageOperationResult> UpdateAsync(UpdatePackageRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        if (decimal.Round(request.Price, 2) != request.Price)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = "Fiyat en fazla iki ondalık basamak içerebilir."
            };
        }

        var package = await _context.SubscriptionPackages
            .FirstOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (package is null)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = "Paket bulunamadı."
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.SubscriptionPackages
            .AnyAsync(
                x => x.Name == name && x.Id != request.Id,
                cancellationToken);

        if (exists)
        {
            return new PackageOperationResult
            {
                Succeeded = false,
                Error = "Bu isimde başka bir paket mevcut."
            };
        }

        package.Name = name;

        package.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        package.Price = request.Price;
        package.DurationInDays = request.DurationInDays;
        package.AccessLevel = request.AccessLevel;
        package.IsActive = request.IsActive;

        package.MarkAsUpdated();

        await _context.SaveChangesAsync(cancellationToken);

        return new PackageOperationResult
        {
            Succeeded = true
        };
    }
}