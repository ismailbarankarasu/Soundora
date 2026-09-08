using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Soundora.Domain.Enums;

namespace Soundora.Presentation.Models.Music;

public class CreateMusicViewModel
{
    [Required(ErrorMessage = "Müzik adı zorunludur.")]
    [StringLength(
        150,
        ErrorMessage = "Müzik adı en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Kategori seçiniz.")]
    public Guid? CategoryId { get; set; }

    public Guid? ArtistId { get; set; }

    [Range(1, 2, ErrorMessage = "Basic veya Gold paketini seçiniz.")]
    public AccessLevel RequiredAccessLevel { get; set; }
        = AccessLevel.Basic;

    [Required(ErrorMessage = "Bir MP3 dosyası seçiniz.")]
    public IFormFile? AudioFile { get; set; }

    public IFormFile? CoverImage { get; set; }

    public List<SelectListItem> Categories { get; set; } = new();

    public List<SelectListItem> Artists { get; set; } = new();
}