using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;

public class TravelClaimQueryHandler(
    IClaimRepository repository,
    ILogger<TravelClaimQueryHandler> logger) : 
    IRequestHandler<GetTravelClaimByIdQuery, TravelClaim?>,
    IRequestHandler<GetAllTravelClaimsQuery, IEnumerable<TravelClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly ILogger<TravelClaimQueryHandler> _logger = logger;

    public async Task<TravelClaim?> Handle(GetTravelClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = (await _repository.GetById(request.Id)) as TravelClaim;
        
        if (claim == null)
        {
            _logger.LogWarning("Travel claim not found: ClaimId={ClaimId}", request.Id);
        }
        
        return claim;
    }

    public async Task<IEnumerable<TravelClaim>> Handle(GetAllTravelClaimsQuery request, CancellationToken cancellationToken)
        => (await _repository.GetByType(ClaimType.Travel)).OfType<TravelClaim>();
}