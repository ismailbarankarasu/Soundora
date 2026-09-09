using System.ComponentModel.DataAnnotations;
using Soundora.Domain.Enums;

namespace Soundora.Application.Packages.Models;

public class CreatePackageRequest
{
    [Required(ErrorMessage = "Paket adı zorunludur.")]
    [StringLength(
        100,
        ErrorMessage = "Paket adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Description { get; set; }

    [Range(
        typeof(decimal),
        "0",
        "999999",
        ErrorMessage = "Fiyat 0 ile 999999 arasında olmalıdır.")]
    public decimal Price { get; set; }

    [Range(
        1,
        3650,
        ErrorMessage = "Paket süresi 1 ile 3650 gün arasında olmalıdır.")]
    public int DurationInDays { get; set; } = 30;

    [Range(1, 2, ErrorMessage = "Basic veya Gold seviyesi seçiniz.")]
    public AccessLevel AccessLevel { get; set; }
        = AccessLevel.Basic;
}