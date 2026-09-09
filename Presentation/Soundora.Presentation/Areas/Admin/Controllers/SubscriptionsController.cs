using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Subscriptions.Abstractions;
using Soundora.Presentation.Models.Subscriptions;

namespace Soundora.Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SubscriptionsController : Controller
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(
        ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionService.GetAllAsync(
            cancellationToken);

        return View(subscriptions);
    }

    [HttpGet]
    public async Task<IActionResult> Assign(CancellationToken cancellationToken)
    {
        var model = new AssignPackageViewModel();

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(AssignPackageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(model, cancellationToken);
            return View(model);
        }

        var result = await _subscriptionService.AssignAsync(
            model.Input,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Paket atanamadı.");

            await PopulateListsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Paket kullanıcıya başarıyla atandı.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateListsAsync(AssignPackageViewModel model, CancellationToken cancellationToken)
    {
        var users = await _subscriptionService.GetUsersAsync(
            cancellationToken);

        model.Users = users
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.UserName
            })
            .ToList();

        var packages = await _subscriptionService.GetPackagesAsync(
            cancellationToken);

        model.Packages = packages
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} — {x.DurationInDays} gün"
            })
            .ToList();
    }


}