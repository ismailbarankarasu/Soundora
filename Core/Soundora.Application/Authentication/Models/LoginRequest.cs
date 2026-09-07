using System.ComponentModel.DataAnnotations;

namespace Soundora.Application.Authentication.Models;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}