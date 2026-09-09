using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Music.Abstractions;

namespace Soundora.Presentation.ViewComponents;

public class _HomeLatestMusicComponentPartial : ViewComponent
{
    private readonly IMusicService _musicService;

    public _HomeLatestMusicComponentPartial(IMusicService musicService)
    {
        _musicService = musicService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string? search = null, int page = 1)
    {
        var result = await _musicService.GetCatalogAsync(search, page, HttpContext.RequestAborted);

        return View(result);
    }
}