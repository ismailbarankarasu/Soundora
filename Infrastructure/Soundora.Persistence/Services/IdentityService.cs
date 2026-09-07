using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Soundora.Application.Authentication.Abstractions;
using Soundora.Application.Authentication.Models;
using Soundora.Persistence.Contexts;
using Soundora.Persistence.Identity;

namespace Soundora.Persistence.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly SignInManager<AppUser> _signInManager;

    public IdentityService(
        UserManager<AppUser> userManager,
        AppDbContext context,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _context = context;
        _signInManager = signInManager;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResult
            {
                Error = "Kullanıcı adı ve şifre zorunludur."
            };
        }

        var result = await _signInManager.PasswordSignInAsync(
            request.UserName.Trim(),
            request.Password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return new LoginResult
            {
                Succeeded = true
            };
        }

        return new LoginResult
        {
            Error = result.IsLockedOut
                ? "Çok fazla başarısız giriş yapıldı. 5 dakika sonra tekrar deneyin."
                : "Kullanıcı adı veya şifre hatalıdır."
        };
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<RegisterResult> RegisterAsync(
        RegisterRequest request)
    {
        var validationResults = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(
            request,
            new ValidationContext(request),
            validationResults,
            validateAllProperties: true);

        if (!isValid)
        {
            return RegisterResult.Failure(
                validationResults.Select(x =>
                    x.ErrorMessage ?? "Geçersiz alan."));
        }

        var userName = request.UserName.Trim();
        var email = request.Email.Trim();

        if (await _userManager.FindByNameAsync(userName) is not null)
        {
            return RegisterResult.Failure(
                new[] { "Bu kullanıcı adı zaten kullanılıyor." });
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return RegisterResult.Failure(
                new[] { "Bu e-posta adresi zaten kullanılıyor." });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            UserName = userName,
            Email = email
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return RegisterResult.Failure(
                createResult.Errors.Select(x => x.Description));
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            "Member");

        if (!roleResult.Succeeded)
        {
            return RegisterResult.Failure(
                roleResult.Errors.Select(x => x.Description));
        }

        await transaction.CommitAsync();

        return RegisterResult.Success();
    }
}