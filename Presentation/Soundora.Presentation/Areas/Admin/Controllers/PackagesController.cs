using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Packages.Abstractions;
using Soundora.Application.Packages.Models;

namespace Soundora.Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PackagesController : Controller
{
    private readonly IPackageService _packageService;

    public PackagesController(IPackageService packageService)
    {
        _packageService = packageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var packages = await _packageService.GetAllAsync(cancellationToken);

        return View(packages);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreatePackageRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePackageRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _packageService.CreateAsync(request, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Paket oluşturulamadı.");

            return View(request);
        }

        TempData["Success"] = "Paket başarıyla oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
    Guid id,
    CancellationToken cancellationToken)
    {
        var model = await _packageService.GetForUpdateAsync(
            id,
            cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdatePackageRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _packageService.UpdateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Paket güncellenemedi.");

            return View(request);
        }

        TempData["Success"] = "Paket başarıyla güncellendi.";

        return RedirectToAction(nameof(Index));
    }
}