using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Music.Models;

namespace Soundora.Presentation.Models.Music;

public class UpdateMusicViewModel
{
    public UpdateMusicRequest Input { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    public List<SelectListItem> Artists { get; set; } = new();
    public IFormFile? CoverImage { get; set; }
}