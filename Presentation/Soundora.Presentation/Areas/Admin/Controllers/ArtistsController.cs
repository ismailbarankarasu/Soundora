using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Artists.Abstractions;
using Soundora.Application.Artists.Models;

namespace Soundora.Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ArtistsController : Controller
{
    private readonly IArtistService _artistService;

    public ArtistsController(IArtistService artistService)
    {
        _artistService = artistService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var artists = await _artistService.GetAllAsync(
            cancellationToken);

        return View(artists);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateArtistRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateArtistRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _artistService.CreateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Sanatçı oluşturulamadı.");

            return View(request);
        }

        TempData["Success"] = "Sanatçı başarıyla oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(
    Guid id,
    CancellationToken cancellationToken)
    {
        var artist = await _artistService.GetByIdAsync(
            id,
            cancellationToken);

        if (artist is null)
        {
            return NotFound();
        }

        var model = new UpdateArtistRequest
        {
            Id = artist.Id,
            Name = artist.Name,
            IsActive = artist.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateArtistRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var result = await _artistService.UpdateAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Sanatçı güncellenemedi.");

            return View(request);
        }

        TempData["Success"] = "Sanatçı başarıyla güncellendi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _artistService.DeleteAsync(id, cancellationToken);

        if (result.Succeeded)
        {
            TempData["Success"] = "Sanatçı başarıyla silindi.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "Sanatçı silinemedi.";
        }

        return RedirectToAction(nameof(Index));
    }
}