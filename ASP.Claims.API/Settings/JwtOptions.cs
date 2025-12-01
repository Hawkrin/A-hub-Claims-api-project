namespace ASP.Claims.API.Settings;

public class JwtOptions
{
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
}
