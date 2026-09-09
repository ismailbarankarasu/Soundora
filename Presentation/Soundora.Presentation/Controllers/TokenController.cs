using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Authentication.Abstractions;
using Soundora.Application.Authentication.Constants;
using Soundora.Application.Authentication.Models;
using Soundora.Application.Subscriptions.Abstractions;
using Soundora.Persistence.Identity;

namespace Soundora.Presentation.Controllers;

public class TokenController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ISubscriptionService _subscriptionService;

    public TokenController(
        UserManager<AppUser> userManager,
        IJwtTokenGenerator tokenGenerator,
        ISubscriptionService subscriptionService)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        Response.Headers["Cache-Control"] = "no-store";

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await _userManager.GetRolesAsync(user);

        var subscription =
            await _subscriptionService.GetActiveForTokenAsync(
                user.Id,
                cancellationToken);

        var result = _tokenGenerator.Generate(new JwtUserInfo
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToArray(),
            Subscription = subscription
        });

        return Json(result);
    }

    [HttpGet]
    [Authorize(Policy = "PlaybackJwt")]
    public IActionResult Verify()
    {
        Response.Headers["Cache-Control"] = "no-store";

        return Json(new
        {
            message = "JWT başarıyla doğrulandı.",
            userId = User.FindFirst("sub")?.Value,
            userName = User.Identity?.Name,

            roles = User.FindAll("role")
                .Select(x => x.Value)
                .ToArray(),

            subscriptionId = User.FindFirst(
                JwtClaimNames.SubscriptionId)?.Value,

            packageId = User.FindFirst(
                JwtClaimNames.PackageId)?.Value,

            packageLevel = User.FindFirst(
                JwtClaimNames.PackageLevel)?.Value,

            packageExpiresAt = User.FindFirst(
                JwtClaimNames.PackageExpiresAt)?.Value
        });
    }
}