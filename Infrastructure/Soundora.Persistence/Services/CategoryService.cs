using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Categories.Abstractions;
using Soundora.Application.Categories.Models;
using Soundora.Domain.Entities;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CreateCategoryResult> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.Categories
            .AnyAsync(x => x.Name == name, cancellationToken);

        if (exists)
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Error = "Bu isimde bir kategori zaten mevcut."
            };
        }

        var category = new Category
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            IsActive = true
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult
        {
            Succeeded = true
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CreateCategoryResult> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var validationResults = new List<ValidationResult>();

        if(!Validator.TryValidateObject(request, new ValidationContext(request), validationResults, validateAllProperties : true))
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Error = validationResults[0].ErrorMessage
            };
        }

        var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if(category is null)
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Error = "Kategori Bulunamadı."
            };
        }

        var name = request.Name.Trim();

        var exists = await _context.Categories.AnyAsync(x => x.Name == name && x.Id != request.Id, cancellationToken);

        if (exists)
        {
            return new CreateCategoryResult
            {
                Succeeded = false,
                Error = "Bu isimde başka bir kategori mevcut !"
            };
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        category.IsActive = request.IsActive;
        category.MarkAsUpdated();

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResult
        {
            Succeeded = true
        };
    }

    public async Task<DeleteCategoryResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (category is null)
        {
            return new DeleteCategoryResult
            {
                Succeeded = false,
                Error = "Kategori bulunamadı."
            };
        }

        var hasContent = await _context.AudioContents
            .AnyAsync(
                x => x.CategoryId == id,
                cancellationToken);

        if (hasContent)
        {
            return new DeleteCategoryResult
            {
                Succeeded = false,
                Error = "Bu kategoriye bağlı müzik veya podcast bulunduğu için silinemez."
            };
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryResult
        {
            Succeeded = true
        };
    }
}