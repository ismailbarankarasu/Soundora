using System.Data;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Subscriptions.Abstractions;
using Soundora.Application.Subscriptions.Models;
using Soundora.Domain.Entities;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;

namespace Soundora.Persistence.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;

    public SubscriptionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SubscriptionUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .Select(x => new SubscriptionUserDto
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubscriptionPackageDto>> GetPackagesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SubscriptionPackages
            .AsNoTracking()
            .Where(x => x.IsActive &&
                        x.DurationInDays > 0 &&
                        (x.AccessLevel == AccessLevel.Basic ||
                         x.AccessLevel == AccessLevel.Gold))
            .OrderBy(x => x.AccessLevel)
            .Select(x => new SubscriptionPackageDto
            {
                Id = x.Id,
                Name = x.Name,
                DurationInDays = x.DurationInDays
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionOperationResult> AssignAsync(AssignPackageRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.UserId.HasValue ||
            request.UserId.Value == Guid.Empty ||
            !request.PackageId.HasValue ||
            request.PackageId.Value == Guid.Empty)
        {
            return new SubscriptionOperationResult
            {
                Succeeded = false,
                Error = "Kullanıcı ve paket seçiniz."
            };
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var userExists = await _context.Users
            .AnyAsync(
                x => x.Id == request.UserId.Value,
                cancellationToken);

        if (!userExists)
        {
            return new SubscriptionOperationResult
            {
                Succeeded = false,
                Error = "Kullanıcı bulunamadı."
            };
        }

        var package = await _context.SubscriptionPackages
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.PackageId.Value && x.IsActive,
                cancellationToken);

        if (package is null)
        {
            return new SubscriptionOperationResult
            {
                Succeeded = false,
                Error = "Paket bulunamadı veya aktif değil."
            };
        }

        if (package.DurationInDays <= 0 ||
            (package.AccessLevel != AccessLevel.Basic &&
             package.AccessLevel != AccessLevel.Gold))
        {
            return new SubscriptionOperationResult
            {
                Succeeded = false,
                Error = "Paketin süresi veya erişim seviyesi geçersiz."
            };
        }

        var now = DateTimeOffset.UtcNow;

        if (package.DurationInDays > (DateTimeOffset.MaxValue - now).TotalDays)
        {
            return new SubscriptionOperationResult
            {
                Succeeded = false,
                Error = "Paket süresi desteklenen tarih aralığını aşıyor."
            };
        }

        var previousSubscriptions = await _context.UserSubscriptions
            .Where(x => x.UserId == request.UserId.Value &&
                        x.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var subscription in previousSubscriptions)
        {
            subscription.IsActive = false;
            subscription.MarkAsUpdated();
        }

        var newSubscription = new UserSubscription
        {
            UserId = request.UserId.Value,
            SubscriptionPackageId = package.Id,
            StartDate = now,
            EndDate = now.AddDays(package.DurationInDays),
            IsActive = true
        };

        _context.UserSubscriptions.Add(newSubscription);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new SubscriptionOperationResult
        {
            Succeeded = true
        };
    }

    public async Task<IReadOnlyList<SubscriptionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await (
            from subscription in _context.UserSubscriptions.AsNoTracking()
            join user in _context.Users.AsNoTracking()
                on subscription.UserId equals user.Id
            orderby subscription.StartDate descending, subscription.Id descending
            select new SubscriptionDto
            {
                Id = subscription.Id,
                UserName = user.UserName ?? string.Empty,
                PackageName = subscription.SubscriptionPackage.Name,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,

                Status = !subscription.IsActive
                    ? "Pasif"
                    : subscription.EndDate <= now
                        ? "Süresi dolmuş"
                        : !subscription.SubscriptionPackage.IsActive
                            ? "Paket pasif"
                            : subscription.StartDate > now
                                ? "Henüz başlamadı"
                                : "Aktif"
            })
            .ToListAsync(cancellationToken);
    }
}