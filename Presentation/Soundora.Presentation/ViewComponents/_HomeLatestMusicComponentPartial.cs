using Microsoft.AspNetCore.Mvc;
using Soundora.Application.Music.Abstractions;

namespace Soundora.Presentation.ViewComponents;

public class _HomeLatestMusicComponentPartial : ViewComponent
{
    private readonly IMusicService _musicService;

    public _HomeLatestMusicComponentPartial(
        IMusicService musicService)
    {
        _musicService = musicService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var musicList = await _musicService.GetLatestAsync(HttpContext.RequestAborted);

        return View(musicList);
    }
}