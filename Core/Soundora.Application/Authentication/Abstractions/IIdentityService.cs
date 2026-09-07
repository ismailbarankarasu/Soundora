using Soundora.Application.Authentication.Models;

namespace Soundora.Application.Authentication.Abstractions;

public interface IIdentityService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}