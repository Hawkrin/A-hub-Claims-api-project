namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreatePropertyClaimHandler(
    IClaimRepository repository, 
    IMapper mapper, 
    IClaimStatusEvaluator claimStatusEvaluator,
    ILogger<CreatePropertyClaimHandler> logger) : 
    IRequestHandler<CreatePropertyClaimCommand, Result<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly ILogger<CreatePropertyClaimHandler> _logger = logger;
    private const decimal HighValueThreshold = 50000m;

    public async Task<Result<PropertyClaim>> Handle(CreatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = _mapper.Map<PropertyClaim>(command);
        claim.Id = Guid.NewGuid();

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        var saveResult = await _repository.Save(claim);

        if (saveResult.IsFailed)
        {
            _logger.LogError("Failed to create property claim: {Error}", saveResult.Errors[0].Message);
            return Result.Fail<PropertyClaim>(saveResult.Errors[0].Message);
        }

        // Log high-value claims for business monitoring and fraud detection
        if (command.EstimatedDamageCost >= HighValueThreshold)
        {
            _logger.LogWarning("High-value property claim created: ClaimId={ClaimId}, Amount={Amount:C}, Address={Address}, Status={Status}", 
                claim.Id, command.EstimatedDamageCost, command.Address, claim.Status);
        }
        else
        {
            _logger.LogInformation("Property claim created: ClaimId={ClaimId}, Amount={Amount:C}, Status={Status}", 
                claim.Id, command.EstimatedDamageCost, claim.Status);
        }

        return Result.Ok(claim);
    }
}
