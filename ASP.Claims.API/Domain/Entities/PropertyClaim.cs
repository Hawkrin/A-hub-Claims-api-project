using ASP.Claims.API.Domain.Enums;

namespace ASP.Claims.API.Domain.Entities;

public class PropertyClaim : Claim
{
    public string Address { get; set; } = string.Empty;

    public PropertyDamageType PropertyDamageType { get; set; }

    public decimal EstimatedDamageCost { get; set; }

    public PropertyClaim()
    {
        Type = ClaimType.Property;
    }
}
