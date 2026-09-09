using Soundora.Application.Authentication.Models;
using Soundora.Application.Subscriptions.Models;

namespace Soundora.Application.Subscriptions.Abstractions;

public interface ISubscriptionService
{
    Task<IReadOnlyList<SubscriptionUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubscriptionPackageDto>> GetPackagesAsync(CancellationToken cancellationToken = default);

    Task<SubscriptionOperationResult> AssignAsync(AssignPackageRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionDto?> GetLatestForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<JwtSubscriptionInfo?> GetActiveForTokenAsync(Guid userId, CancellationToken cancellationToken = default);
}