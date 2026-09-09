using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Subscriptions.Abstractions;

namespace Soundora.Presentation.Controllers;

[Authorize(AuthenticationSchemes = "Identity.Application")]
public class SubscriptionController : Controller
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId) || userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var subscription = await _subscriptionService.GetLatestForUserAsync(userId, cancellationToken);

        Response.Headers["Cache-Control"] = "no-store";

        return View(subscription);
    }
}