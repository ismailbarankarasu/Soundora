using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Application.Podcasts.Models;

namespace Soundora.Presentation.Models.Podcasts;

public class UpdatePodcastViewModel
{
    public UpdatePodcastRequest Input { get; set; } = new();

    public List<SelectListItem> Categories { get; set; } = new();

    public List<SelectListItem> Hosts { get; set; } = new();
}