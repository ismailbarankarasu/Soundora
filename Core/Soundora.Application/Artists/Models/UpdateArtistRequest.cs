using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Artists.Models;

public class UpdateArtistRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Sanatçı adı zorunludur.")]
    [StringLength(
        100,
        ErrorMessage = "Sanatçı adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}