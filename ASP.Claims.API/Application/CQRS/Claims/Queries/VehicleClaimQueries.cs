using MediatR;
using ASP.Claims.API.Domain.Entities;

namespace ASP.Claims.API.Application.CQRS.Claims.Queries;

public record GetVehicleClaimByIdQuery(Guid Id) : IRequest<VehicleClaim?>;
public record GetAllVehicleClaimsQuery() : IRequest<IEnumerable<VehicleClaim>>;