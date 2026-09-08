using Soundora.Application.Authentication.Models;

namespace Soundora.Application.Authentication.Abstractions;

public interface IJwtTokenGenerator
{
    JwtTokenResult Generate(JwtUserInfo userInfo);
}