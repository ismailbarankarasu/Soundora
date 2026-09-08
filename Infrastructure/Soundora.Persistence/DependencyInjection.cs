using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Soundora.Application.Artists.Abstractions;
using Soundora.Application.Authentication.Abstractions;
using Soundora.Application.Categories.Abstractions;
using Soundora.Application.Music.Abstractions;
using Soundora.Application.Subscriptions.Abstractions;
using Soundora.Persistence.Contexts;
using Soundora.Persistence.Identity;
using Soundora.Persistence.Seeds;
using Soundora.Persistence.Services;

namespace Soundora.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Veritabanı bağlantı adresi bulunamadı.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services
            .AddIdentity<AppUser, AppRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<DataSeeder>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMusicService, MusicService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IArtistService, ArtistService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        return services;
    }
}