using Soundora.Application.Packages.Models;

namespace Soundora.Application.Packages.Abstractions;

public interface IPackageService
{
    Task<IReadOnlyList<PackageDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PackageOperationResult> CreateAsync(CreatePackageRequest request, CancellationToken cancellationToken = default);
    Task<UpdatePackageRequest?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PackageOperationResult> UpdateAsync(UpdatePackageRequest request, CancellationToken cancellationToken = default);
    Task<PackageOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}