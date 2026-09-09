using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Artists.Abstractions;
using Soundora.Application.Categories.Abstractions;
using Soundora.Application.Files.Abstractions;
using Soundora.Application.Music.Abstractions;
using Soundora.Application.Music.Models;
using Soundora.Presentation.Models.Music;

namespace Soundora.Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Manager")]
public class MusicController : Controller
{
    private readonly IMusicService _musicService;
    private readonly ICategoryService _categoryService;
    private readonly IArtistService _artistService;
    private readonly IAudioFileStorage _audioFileStorage;
    private readonly ILogger<MusicController> _logger;

    public MusicController(
        IMusicService musicService,
        ICategoryService categoryService,
        IArtistService artistService,
        IAudioFileStorage audioFileStorage,
        ILogger<MusicController> logger)
    {
        _musicService = musicService;
        _categoryService = categoryService;
        _artistService = artistService;
        _audioFileStorage = audioFileStorage;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
    CancellationToken cancellationToken)
    {
        var musicList = await _musicService.GetAllAsync(cancellationToken);
        return View(musicList);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new CreateMusicViewModel();

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(25 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 25 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        CreateMusicViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.AudioFile is not null)
        {
            if (model.AudioFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.AudioFile),
                    "Boş dosya yükleyemezsiniz.");
            }
            else if (model.AudioFile.Length > 20 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.AudioFile),
                    "MP3 dosyası en fazla 20 MB olabilir.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(model, cancellationToken);
            return View(model);
        }

        string? uploadedFilePath = null;

        try
        {
            await using var stream = model.AudioFile!.OpenReadStream();

            var storedFile = await _audioFileStorage.SaveAsync(
                stream,
                model.AudioFile.FileName,
                cancellationToken);

            uploadedFilePath = storedFile.FilePath;

            var result = await _musicService.CreateAsync(
                new CreateMusicRequest
                {
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId!.Value,
                    ArtistId = model.ArtistId,
                    RequiredAccessLevel = model.RequiredAccessLevel,
                    FilePath = storedFile.FilePath,
                    DurationInSeconds = storedFile.DurationInSeconds
                },
                cancellationToken);

            if (!result.Succeeded)
            {
                await CleanupFileAsync(uploadedFilePath);
                uploadedFilePath = null;

                ModelState.AddModelError(
                    string.Empty,
                    result.Error ?? "Müzik oluşturulamadı.");
            }
            else
            {
                uploadedFilePath = null;

                TempData["Success"] = "Müzik başarıyla eklendi.";

                return RedirectToAction(nameof(Index));
            }
        }
        catch (InvalidDataException exception)
        {
            await CleanupFileAsync(uploadedFilePath);

            ModelState.AddModelError(
                nameof(model.AudioFile),
                exception.Message);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Müzik eklenirken hata oluştu. Dosya: {FilePath}",
                uploadedFilePath);

            ModelState.AddModelError(
                string.Empty,
                "İşlem tamamlanamadı. Tekrar denemeden önce kaydın oluşup oluşmadığını kontrol edin.");
        }

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }

    private async Task PopulateListsAsync(
        CreateMusicViewModel model,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(
            cancellationToken);

        model.Categories = categories
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();

        var artists = await _artistService.GetAllAsync(
            cancellationToken);

        model.Artists = artists
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();
    }

    private async Task CleanupFileAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            await _audioFileStorage.DeleteAsync(
                filePath,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Kullanılmayan ses dosyası temizlenemedi: {FilePath}",
                filePath);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var music = await _musicService.GetForUpdateAsync(
            id,
            cancellationToken);

        if (music is null)
        {
            return NotFound();
        }

        var model = new UpdateMusicViewModel
        {
            Input = music
        };

        await PopulateEditListsAsync(
            model,
            music.CategoryId,
            music.ArtistId,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        Guid id,
        UpdateMusicViewModel model,
        CancellationToken cancellationToken)
    {
        if (id != model.Input.Id)
        {
            return BadRequest();
        }

        var currentMusic = await _musicService.GetForUpdateAsync(
            id,
            cancellationToken);

        if (currentMusic is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateEditListsAsync(
                model,
                currentMusic.CategoryId,
                currentMusic.ArtistId,
                cancellationToken);

            return View(model);
        }

        var result = await _musicService.UpdateAsync(
            model.Input,
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Müzik güncellenemedi.");

            await PopulateEditListsAsync(
                model,
                currentMusic.CategoryId,
                currentMusic.ArtistId,
                cancellationToken);

            return View(model);
        }

        TempData["Success"] = "Müzik başarıyla güncellendi.";

        return RedirectToAction(nameof(Index));
    }
    private async Task PopulateEditListsAsync(UpdateMusicViewModel model, Guid? currentCategoryId, Guid? currentArtistId, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        model.Categories = categories
            .Where(x => x.IsActive || x.Id == currentCategoryId)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.IsActive
                    ? x.Name
                    : $"{x.Name} (Pasif)"
            })
            .ToList();

        var artists = await _artistService.GetAllAsync(
            cancellationToken);

        model.Artists = artists
            .Where(x => x.IsActive || x.Id == currentArtistId)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.IsActive
                    ? x.Name
                    : $"{x.Name} (Pasif)"
            })
            .ToList();
    }
}