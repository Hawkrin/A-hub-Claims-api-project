using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Domain.Enums;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.QueryHandlers;

public class PropertyClaimQueryHandler(
    IClaimRepository repository,
    ILogger<PropertyClaimQueryHandler> logger) : 
    IRequestHandler<GetPropertyClaimByIdQuery, PropertyClaim?>,
    IRequestHandler<GetAllPropertyClaimsQuery, IEnumerable<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly ILogger<PropertyClaimQueryHandler> _logger = logger;

    public async Task<PropertyClaim?> Handle(GetPropertyClaimByIdQuery request, CancellationToken cancellationToken)
    {
        var claim = (await _repository.GetById(request.Id)) as PropertyClaim;
        
        if (claim == null)
        {
            _logger.LogWarning("Property claim not found: ClaimId={ClaimId}", request.Id);
        }
        
        return claim;
    }

    public async Task<IEnumerable<PropertyClaim>> Handle(GetAllPropertyClaimsQuery request, CancellationToken cancellationToken)
        => (await _repository.GetByType(ClaimType.Property)).OfType<PropertyClaim>();
}