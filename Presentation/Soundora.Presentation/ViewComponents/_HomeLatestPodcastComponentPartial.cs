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

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var podcasts = await _podcastService.GetLatestAsync(HttpContext.RequestAborted);
        return View(podcasts);
    }
}