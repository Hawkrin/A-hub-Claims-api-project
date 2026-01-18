using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Claim = System.Security.Claims.Claim;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserRepository userRepo, ITokenKeyProvider tokenKeyProvider, IOptions<JwtOptions> jwtOptionsAccessor) : ControllerBase
{
    private readonly IUserRepository _userRepo = userRepo;
    private readonly ITokenKeyProvider _tokenKeyProvider = tokenKeyProvider;
    private readonly JwtOptions _jwtOptions = jwtOptionsAccessor.Value;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var existing = await _userRepo.GetByUsernameAsync(dto.Username);
        if (existing != null)
            return BadRequest(new { error = "Username already exists." });

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
        };

        var result = await _userRepo.SaveAsync(user);
        if (result.IsFailed)
            return BadRequest(new { error = result.Errors[0].Message });

        return Ok(new { message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto login)
    {
        var user = await _userRepo.GetByUsernameAsync(login.Username);
        if (user == null)
            return Unauthorized(new { error = "Invalid username or password." });

        if (!BCrypt.Net.BCrypt.Verify(login.Password, user.Password))
            return Unauthorized(new { error = "Invalid username or password." });

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString() ?? "User")
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

    [HttpPost("token")]
    public async Task<IActionResult> GenerateToken([FromBody] LoginDto login)
    {
        return await Login(login);
    }
}
