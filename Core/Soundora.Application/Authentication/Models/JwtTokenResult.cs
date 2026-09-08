namespace Soundora.Application.Authentication.Models;

public class JwtTokenResult
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; init; }
}