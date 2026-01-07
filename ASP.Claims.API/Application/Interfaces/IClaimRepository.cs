using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using FluentResults;

namespace ASP.Claims.API.Application.Interfaces;

public interface IClaimRepository
{
    Task<Result<Claim>> Save(Claim claim);
    Task<Claim?> GetById(Guid id);
    Task<IEnumerable<Claim>> GetAll();
    Task<IEnumerable<Claim>> GetByType(ClaimType type);
    Task<Result<Claim>> UpdateClaim(Claim claim);
    Task<Result> DeleteClaim(Guid id);
}