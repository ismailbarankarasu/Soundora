using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Soundora.Presentation.Models.Podcasts;

public class CreatePodcastViewModel
{
    [Required(ErrorMessage = "Podcast adı zorunludur.")]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Kategori seçiniz.")]
    public Guid? CategoryId { get; set; }

    public Guid? ArtistId { get; set; }

    [Required(ErrorMessage = "Bir MP3 dosyası seçiniz.")]
    public IFormFile? AudioFile { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    public List<SelectListItem> Hosts { get; set; } = new();
}