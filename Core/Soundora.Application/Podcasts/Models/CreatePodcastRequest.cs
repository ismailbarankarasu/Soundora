using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Podcasts.Models;

public class CreatePodcastRequest
{
    [Required(ErrorMessage = "Podcast adı zorunludur.")]
    [StringLength(
        150,
        ErrorMessage = "Podcast adı en fazla 150 karakter olabilir.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(
        2000,
        ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? ArtistId { get; set; }

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Geçerli bir podcast süresi gereklidir.")]
    public int DurationInSeconds { get; set; }
}