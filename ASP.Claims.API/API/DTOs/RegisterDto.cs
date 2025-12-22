using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.API.DTOs;

public class RegisterDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required Role Role { get; set; }
}
