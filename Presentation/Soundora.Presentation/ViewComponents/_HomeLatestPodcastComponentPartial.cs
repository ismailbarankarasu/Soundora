using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Podcasts.Abstractions;

namespace Soundora.Presentation.ViewComponents;

public class _HomeLatestPodcastComponentPartial : ViewComponent
{
    private readonly IPodcastService _podcastService;

    public _HomeLatestPodcastComponentPartial(IPodcastService podcastService)
    {
        _podcastService = podcastService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? search = null, int page = 1)
    {
        var result = await _podcastService.GetCatalogAsync(search, page, HttpContext.RequestAborted);

        return View(result);
    }
}