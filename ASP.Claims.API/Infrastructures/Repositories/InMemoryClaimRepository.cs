using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using ASP.Claims.API.Resources;
using FluentResults;

namespace ASP.Claims.API.Infrastructures.Repositories;

public class InMemoryClaimRepository : IClaimRepository
{
    private readonly List<Claim> _claims = [];

    public Task<IEnumerable<Claim>> GetAll()
        => Task.FromResult<IEnumerable<Claim>>(_claims);

    public Task<IEnumerable<Claim>> GetByType(ClaimType type)
    {
        var filteredClaims = _claims.Where(c => c.Type == type);
        return Task.FromResult(filteredClaims);
    }

    public Task<Claim?> GetById(Guid id)
    {
        var claim = _claims.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(claim);
    }

    public Task<Result<Claim>> Save(Claim claim)
    {
        try
        {
            _claims.Add(claim);
            return Task.FromResult(Result.Ok(claim));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Fail<Claim>(ErrorMessages.ErrorMessage_FailedToCreateClaim));
        }
    }

    public Task<Result<Claim>> UpdateClaim(Claim claim)
    {
        var index = _claims.FindIndex(c => c.Id == claim.Id);

        if (index >= 0)
        {
            _claims[index] = claim;
            return Task.FromResult(Result.Ok(claim));
        }
        else
        {
            return Task.FromResult(Result.Fail<Claim>(ErrorMessages.ErrorMessage_ClaimNotFound));
        }
    }

    public Task<Result> DeleteClaim(Guid id)
    {
        var claim = _claims.FirstOrDefault(c => c.Id == id);
        if (claim is not null)
        {
            _claims.Remove(claim);
            return Task.FromResult(Result.Ok());
        }
        else
        {
            return Task.FromResult(Result.Fail(ErrorMessages.ErrorMessage_ClaimNotFound));
        }
    }

    public InMemoryClaimRepository()
    {
        // Property Claims
        _claims.Add(new PropertyClaim
        {
            Id = Guid.NewGuid(),
            Address = "123 Main St",
            Description = "Fire damage in kitchen",
            EstimatedDamageCost = 5000,
            PropertyDamageType = PropertyDamageType.Fire,
            Type = ClaimType.Property,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-10)
        });
        _claims.Add(new PropertyClaim
        {
            Id = Guid.NewGuid(),
            Address = "456 Oak Ave",
            Description = "Water damage in basement",
            EstimatedDamageCost = 3000,
            PropertyDamageType = PropertyDamageType.Water,
            Type = ClaimType.Property,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-20)
        });

        // Vehicle Claims
        _claims.Add(new VehicleClaim
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "ABC123",
            PlaceOfAccident = "Downtown",
            Description = "Rear-end collision",
            Type = ClaimType.Vehicle,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-5)
        });
        _claims.Add(new VehicleClaim
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = "XYZ789",
            PlaceOfAccident = "Highway",
            Description = "Side swipe",
            Type = ClaimType.Vehicle,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-15)
        });

        // Travel Claims
        _claims.Add(new TravelClaim
        {
            Id = Guid.NewGuid(),
            Country = Country.Angola,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-25),
            IncidentType = IncidentType.LostLuggage,
            Description = "Lost luggage at airport",
            Type = ClaimType.Travel,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-24)
        });
        _claims.Add(new TravelClaim
        {
            Id = Guid.NewGuid(),
            Country = Country.Afghanistan,
            StartDate = DateTime.UtcNow.AddDays(-40),
            EndDate = DateTime.UtcNow.AddDays(-35),
            IncidentType = IncidentType.Medical,
            Description = "Medical emergency during trip",
            Type = ClaimType.Travel,
            Status = ClaimStatus.None,
            ReportedDate = DateTime.UtcNow.AddDays(-34)
        });
    }
}