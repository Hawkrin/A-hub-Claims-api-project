using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Entities;

public class User
{
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required Role Role { get; set; } = Role.User;
}