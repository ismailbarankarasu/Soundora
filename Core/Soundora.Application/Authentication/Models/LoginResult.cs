namespace Soundora.Application.Authentication.Models;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }

    public string? Error { get; init; }
}