using System.ComponentModel.DataAnnotations;
using Soundora.Domain.Enums;

namespace Soundora.Application.Music.Models;

public class CreateMusicRequest
{
    [Required(ErrorMessage = "Müzik adı zorunludur.")]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Guid? ArtistId { get; set; }

    [Range(1, 2, ErrorMessage = "Basic veya Gold paketini seçiniz.")]
    public AccessLevel RequiredAccessLevel { get; set; }
        = AccessLevel.Basic;

    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? CoverImagePath { get; set; }
}