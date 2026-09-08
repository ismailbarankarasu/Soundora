using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Subscriptions.Models;

public class AssignPackageRequest
{
    [Required(ErrorMessage = "Kullanıcı seçiniz.")]
    public Guid? UserId { get; set; }

    [Required(ErrorMessage = "Paket seçiniz.")]
    public Guid? PackageId { get; set; }
}