using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Categories.Models;

public class UpdateCategoryRequest
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Kategori adı zorunludur.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}