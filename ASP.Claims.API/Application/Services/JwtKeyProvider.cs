using ASP.Claims.API.Application.Interfaces;

namespace ASP.Claims.API.Application.Services;

public class JwtKeyProvider(string key) : ITokenKeyProvider
{
    public string Key { get; } = key;
}