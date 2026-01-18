using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;

public class VehicleClaimQueryHandler(
    IClaimRepository repository,
    ILogger<VehicleClaimQueryHandler> logger) : 
    IRequestHandler<GetVehicleClaimByIdQuery, VehicleClaim?>,
    IRequestHandler<GetAllVehicleClaimsQuery, IEnumerable<VehicleClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly ILogger<VehicleClaimQueryHandler> _logger = logger;

    public async Task<VehicleClaim?> Handle(GetVehicleClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = (await _repository.GetById(request.Id)) as VehicleClaim;
        
        if (claim == null)
        {
            _logger.LogWarning("Vehicle claim not found: ClaimId={ClaimId}", request.Id);
        }
        
        return claim;
    }

    public async Task<IEnumerable<VehicleClaim>> Handle(GetAllVehicleClaimsQuery request, CancellationToken cancellationToken)
        => (await _repository.GetByType(ClaimType.Vehicle)).OfType<VehicleClaim>();
}