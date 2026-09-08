using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Soundora.Application.Authentication.Abstractions;
using Soundora.Application.Authentication.Models;

namespace Soundora.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    public JwtTokenGenerator(JwtSettings settings)
    {
        _settings = settings;
    }

    public JwtTokenResult Generate(JwtUserInfo userInfo)
    {
        if (userInfo.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Token üretmek için geçerli bir kullanıcı gereklidir.",
                nameof(userInfo));
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(
            _settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                userInfo.UserId.ToString()),

            new(
                JwtRegisteredClaimNames.UniqueName,
                userInfo.UserName),

            new(
                JwtRegisteredClaimNames.Email,
                userInfo.Email),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),

            new(
                JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        foreach (var role in userInfo.Roles.Distinct())
        {
            claims.Add(new Claim("role", role));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtTokenResult
        {
            AccessToken = new JwtSecurityTokenHandler()
                .WriteToken(token),

            ExpiresAtUtc = expiresAt
        };
    }
}