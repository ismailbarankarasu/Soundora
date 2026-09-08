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
[Authorize(Roles = "Admin")]
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
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = new CreateMusicViewModel();

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
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
                // Kayıt başarılı; dosya artık müzik kaydına ait.
                uploadedFilePath = null;

                TempData["Success"] = "Müzik başarıyla eklendi.";

                return RedirectToAction(nameof(Create));
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
            // Veritabanı işleminin sonucu belirsiz olabilir.
            // Dosyayı burada silerek olası başarılı kaydı bozmayız.
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Müzik eklenirken hata oluştu. Dosya: {FilePath}",
                uploadedFilePath);

            // Veritabanına yazmanın kesin sonucunu bilmediğimiz
            // hatalarda dosyayı otomatik silmiyoruz.
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
}