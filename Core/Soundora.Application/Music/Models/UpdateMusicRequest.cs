using System.ComponentModel.DataAnnotations;
using Soundora.Domain.Enums;

namespace Soundora.Application.Music.Models;

public class UpdateMusicRequest
{
    public Guid Id { get; set; }

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

    public bool IsActive { get; set; }
}