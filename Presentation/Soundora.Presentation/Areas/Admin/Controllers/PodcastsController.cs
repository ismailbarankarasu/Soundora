using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Artists.Abstractions;
using Soundora.Application.Categories.Abstractions;
using Soundora.Application.Files.Abstractions;
using Soundora.Application.Podcasts.Abstractions;
using Soundora.Application.Podcasts.Models;
using Soundora.Presentation.Models.Podcasts;

namespace Soundora.Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PodcastsController : Controller
{
    private readonly IPodcastService _podcastService;
    private readonly ICategoryService _categoryService;
    private readonly IArtistService _artistService;
    private readonly IAudioFileStorage _audioFileStorage;
    private readonly ILogger<PodcastsController> _logger;
    private readonly ICoverFileStorage _coverFileStorage;

    public PodcastsController(
        IPodcastService podcastService,
        ICategoryService categoryService,
        IArtistService artistService,
        IAudioFileStorage audioFileStorage,
        ICoverFileStorage coverFileStorage,
        ILogger<PodcastsController> logger)
    {
        _podcastService = podcastService;
        _categoryService = categoryService;
        _artistService = artistService;
        _audioFileStorage = audioFileStorage;
        _coverFileStorage = coverFileStorage;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var podcasts = await _podcastService.GetAllAsync(cancellationToken);

        return View(podcasts);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new CreatePodcastViewModel();

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(30 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 30 * 1024 * 1024)]
    public async Task<IActionResult> Create(
        CreatePodcastViewModel model,
        CancellationToken cancellationToken)
    {
        if (model.AudioFile is not null)
        {
            if (model.AudioFile.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.AudioFile),
                    "Boş ses dosyası yükleyemezsiniz.");
            }
            else if (model.AudioFile.Length > 20 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.AudioFile),
                    "MP3 dosyası en fazla 20 MB olabilir.");
            }
        }

        if (model.CoverImage is not null)
        {
            if (model.CoverImage.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage),
                    "Boş kapak görseli yükleyemezsiniz.");
            }
            else if (model.CoverImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage),
                    "Kapak görseli en fazla 5 MB olabilir.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(model, cancellationToken);
            return View(model);
        }

        string? audioPath = null;
        string? coverPath = null;

        var savingRecord = false;
        var fileField = nameof(model.CoverImage);

        try
        {
            if (model.CoverImage is not null)
            {
                await using var coverStream =
                    model.CoverImage.OpenReadStream();

                var storedCover = await _coverFileStorage.SaveAsync(
                    coverStream,
                    model.CoverImage.FileName,
                    cancellationToken);

                coverPath = storedCover.FilePath;
            }

            fileField = nameof(model.AudioFile);

            await using var audioStream =
                model.AudioFile!.OpenReadStream();

            var storedAudio = await _audioFileStorage.SaveAsync(
                audioStream,
                model.AudioFile.FileName,
                cancellationToken);

            audioPath = storedAudio.FilePath;
            savingRecord = true;

            var result = await _podcastService.CreateAsync(
                new CreatePodcastRequest
                {
                    Title = model.Title,
                    Description = model.Description,
                    CategoryId = model.CategoryId!.Value,
                    ArtistId = model.ArtistId,
                    FilePath = audioPath,
                    CoverImagePath = coverPath,
                    DurationInSeconds = storedAudio.DurationInSeconds
                },
                cancellationToken);

            if (!result.Succeeded)
            {
                savingRecord = false;

                await CleanupFileAsync(audioPath);
                await CleanupCoverAsync(coverPath);

                audioPath = null;
                coverPath = null;

                ModelState.AddModelError(
                    string.Empty,
                    result.Error ?? "Podcast oluşturulamadı.");
            }
            else
            {
                TempData["Success"] = "Podcast başarıyla eklendi.";

                return RedirectToAction(nameof(Index));
            }
        }
        catch (InvalidDataException exception) when (!savingRecord)
        {
            await CleanupFileAsync(audioPath);
            await CleanupCoverAsync(coverPath);

            ModelState.AddModelError(
                fileField,
                exception.Message);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            if (!savingRecord)
            {
                await CleanupFileAsync(audioPath);
                await CleanupCoverAsync(coverPath);
            }

            throw;
        }
        catch (Exception exception)
        {
            if (!savingRecord)
            {
                await CleanupFileAsync(audioPath);
                await CleanupCoverAsync(coverPath);
            }

            _logger.LogError(
                exception,
                "Podcast eklenemedi. Ses: {AudioPath}, Kapak: {CoverPath}",
                audioPath,
                coverPath);

            ModelState.AddModelError(
                string.Empty,
                savingRecord
                    ? "İşlem sonucu doğrulanamadı. Tekrar denemeden önce podcast listesini kontrol edin."
                    : "Dosyalar yüklenemedi. Lütfen tekrar deneyiniz.");
        }

        await PopulateListsAsync(model, cancellationToken);

        return View(model);
    }
    private async Task CleanupCoverAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        try
        {
            await _coverFileStorage.DeleteAsync(
                filePath,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Kullanılmayan podcast kapağı temizlenemedi: {FilePath}",
                filePath);
        }
    }
    private async Task PopulateListsAsync(CreatePodcastViewModel model, CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);

        model.Categories = categories
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToList();

        var artists = await _artistService.GetAllAsync(cancellationToken);

        model.Hosts = artists
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
                "Kullanılmayan podcast dosyası temizlenemedi: {FilePath}",
                filePath);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var podcast = await _podcastService.GetForUpdateAsync(
            id,
            cancellationToken);

        if (podcast is null)
        {
            return NotFound();
        }

        var model = new UpdatePodcastViewModel
        {
            Input = podcast
        };

        await PopulateEditListsAsync(
            model,
            podcast.CategoryId,
            podcast.ArtistId,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(7 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 7 * 1024 * 1024)]
    public async Task<IActionResult> Edit(Guid id, UpdatePodcastViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Input.Id)
        {
            return BadRequest();
        }

        var currentPodcast = await _podcastService.GetForUpdateAsync(id, cancellationToken);

        if (currentPodcast is null)
        {
            return NotFound();
        }

        model.Input.CoverImagePath = null;
        ModelState.Remove("Input.CoverImagePath");

        if (model.CoverImage is not null)
        {
            if (model.CoverImage.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage),
                    "Boş kapak görseli yükleyemezsiniz.");
            }
            else if (model.CoverImage.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage),
                    "Kapak görseli en fazla 5 MB olabilir.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.Input.CoverImagePath = currentPodcast.CoverImagePath;

            await PopulateEditListsAsync(
                model,
                currentPodcast.CategoryId,
                currentPodcast.ArtistId,
                cancellationToken);

            return View(model);
        }

        string? newCoverPath = null;
        var savingRecord = false;

        try
        {
            if (model.CoverImage is not null)
            {
                await using var stream = model.CoverImage.OpenReadStream();

                var storedCover = await _coverFileStorage.SaveAsync(
                    stream,
                    model.CoverImage.FileName,
                    cancellationToken);

                newCoverPath = storedCover.FilePath;
            }

            model.Input.CoverImagePath = newCoverPath;
            savingRecord = true;

            var result = await _podcastService.UpdateAsync(
                model.Input,
                cancellationToken);

            if (!result.Succeeded)
            {
                savingRecord = false;

                await CleanupCoverAsync(newCoverPath);
                newCoverPath = null;

                ModelState.AddModelError(
                    string.Empty,
                    result.Error ?? "Podcast güncellenemedi.");
            }
            else
            {
                newCoverPath = null;

                if (!string.IsNullOrWhiteSpace(result.PreviousCoverImagePath))
                {
                    try
                    {
                        await _coverFileStorage.DeleteAsync(
                            result.PreviousCoverImagePath,
                            CancellationToken.None);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(
                            exception,
                            "Önceki podcast kapağı temizlenemedi: {FilePath}",
                            result.PreviousCoverImagePath);

                        TempData["Warning"] =
                            "Podcast güncellendi ancak eski kapak dosyası temizlenemedi.";
                    }
                }

                TempData["Success"] = "Podcast başarıyla güncellendi.";

                return RedirectToAction(nameof(Index));
            }
        }
        catch (InvalidDataException exception) when (!savingRecord)
        {
            await CleanupCoverAsync(newCoverPath);

            ModelState.AddModelError(
                nameof(model.CoverImage),
                exception.Message);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            if (!savingRecord)
            {
                await CleanupCoverAsync(newCoverPath);
            }

            throw;
        }
        catch (Exception exception)
        {
            if (!savingRecord)
            {
                await CleanupCoverAsync(newCoverPath);
            }

            _logger.LogError(
                exception,
                "Podcast güncellenemedi. Podcast: {PodcastId}, Kapak: {CoverPath}",
                id,
                newCoverPath);

            ModelState.AddModelError(
                string.Empty,
                savingRecord
                    ? "İşlem sonucu doğrulanamadı. Tekrar denemeden önce podcast listesini kontrol edin."
                    : "Kapak yüklenemedi. Lütfen tekrar deneyiniz.");
        }

        model.Input.CoverImagePath = currentPodcast.CoverImagePath;

        await PopulateEditListsAsync(
            model,
            currentPodcast.CategoryId,
            currentPodcast.ArtistId,
            cancellationToken);

        return View(model);
    }
    private async Task PopulateEditListsAsync(UpdatePodcastViewModel model, Guid? currentCategoryId, Guid? currentArtistId, CancellationToken cancellationToken)
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

        var artists = await _artistService.GetAllAsync(cancellationToken);

        model.Hosts = artists
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _podcastService.DeleteAsync(id, cancellationToken);

        if (!result.Succeeded)
        {
            TempData["Error"] = result.Error ?? "Podcast silinemedi.";

            return RedirectToAction(nameof(Index));
        }

        var cleanupFailed = false;

        if (!string.IsNullOrWhiteSpace(result.FilePath))
        {
            try
            {
                await _audioFileStorage.DeleteAsync(
                    result.FilePath,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                cleanupFailed = true;

                _logger.LogError(
                    exception,
                    "Silinen podcast'in ses dosyası temizlenemedi: {FilePath}",
                    result.FilePath);
            }
        }

        if (!string.IsNullOrWhiteSpace(result.CoverImagePath))
        {
            try
            {
                await _coverFileStorage.DeleteAsync(
                    result.CoverImagePath,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                cleanupFailed = true;

                _logger.LogError(
                    exception,
                    "Silinen podcast'in kapak dosyası temizlenemedi: {FilePath}",
                    result.CoverImagePath);
            }
        }

        if (cleanupFailed)
        {
            TempData["Warning"] = "Podcast kaydı silindi ancak bazı dosyalar temizlenemedi.";
        }
        else
        {
            TempData["Success"] = "Podcast başarıyla silindi.";
        }

        return RedirectToAction(nameof(Index));
    }
}