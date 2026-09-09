using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Podcasts.Models;

public class UpdatePodcastRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Podcast adı zorunludur.")]
    [StringLength(
        150,
        ErrorMessage = "Podcast adı en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Kategori seçiniz.")]
    public Guid? CategoryId { get; set; }

    public Guid? ArtistId { get; set; }

    public bool IsActive { get; set; }
}