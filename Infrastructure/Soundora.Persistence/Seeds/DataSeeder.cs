using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Soundora.Domain.Entities;
using Soundora.Domain.Enums;
using Soundora.Persistence.Contexts;
using Soundora.Persistence.Identity;

namespace Soundora.Persistence.Seeds;

public sealed class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly RoleManager<AppRole> _roleManager;

    public DataSeeder(
        AppDbContext context,
        RoleManager<AppRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedPackagesAsync();
    }

    private async Task SeedRolesAsync()
    {
        string[] roleNames = { "Admin", "Manager", "Member" };

        foreach (var roleName in roleNames)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await _roleManager.CreateAsync(
                new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                });

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description));

                throw new InvalidOperationException(
                    $"{roleName} rolü oluşturulamadı: {errors}");
            }
        }
    }

    private async Task SeedPackagesAsync()
    {
        // Başlangıç paketleri yalnızca paket tablosu boşken eklenir.
        if (await _context.SubscriptionPackages.AnyAsync())
        {
            return;
        }

        _context.SubscriptionPackages.AddRange(
            new SubscriptionPackage
            {
                Name = "Basic",
                Description = "Basic seviyesindeki içeriklere erişim.",
                Price = 49.99m,
                DurationInDays = 30,
                AccessLevel = AccessLevel.Basic,
                IsActive = true
            },
            new SubscriptionPackage
            {
                Name = "Gold",
                Description = "Tüm müzik ve podcast içeriklerine erişim.",
                Price = 99.99m,
                DurationInDays = 30,
                AccessLevel = AccessLevel.Gold,
                IsActive = true
            });

        await _context.SaveChangesAsync();
    }
}