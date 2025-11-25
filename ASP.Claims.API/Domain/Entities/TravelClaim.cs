using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Entities;

public class TravelClaim : Claim
{
    public Country Country { get; set; }

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; } = DateTime.UtcNow;

    public IncidentType IncidentType { get; set; }

    public TravelClaim()
    {
        Type = ClaimType.Travel;
    }
}
