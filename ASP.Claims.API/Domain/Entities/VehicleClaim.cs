using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Entities;

public class VehicleClaim : Claim
{
    public string RegistrationNumber { get; set; } = string.Empty;

    public string PlaceOfAccident { get; set; } = string.Empty;

    public VehicleClaim()
    {
        Type = ClaimType.Vehicle;
    }
}