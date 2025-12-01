using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenKeyProvider tokenKeyProvider, IOptions<JwtOptions> jwtOptionsAccessor) : ControllerBase
{
    private readonly ITokenKeyProvider _tokenKeyProvider = tokenKeyProvider;
    private readonly JwtOptions _jwtOptions = jwtOptionsAccessor.Value;

    [HttpPost("token")]
    public IActionResult GenerateToken([FromBody] LoginDto login)
    {
        if (login.Username != "admin" || login.Password != "password")
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, login.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKeyProvider.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}