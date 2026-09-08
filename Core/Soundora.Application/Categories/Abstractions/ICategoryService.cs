using Soundora.Application.Categories.Models;

namespace Soundora.Application.Categories.Abstractions;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<CreateCategoryResult> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CreateCategoryResult> UpdateAsync(UpdateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<DeleteCategoryResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}