using ASP.Claims.API.Domain.Enums;
using Newtonsoft.Json;

namespace ASP.Claims.API.Domain.Entities;

public class User
{
    [JsonProperty("id")]
    public required string Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required Role Role { get; set; } = Role.User;
}