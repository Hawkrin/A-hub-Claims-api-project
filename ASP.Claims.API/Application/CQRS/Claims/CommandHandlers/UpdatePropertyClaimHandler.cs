using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdatePropertyClaimHandler(
    IClaimRepository repository, 
    IMapper mapper, 
    IClaimStatusEvaluator claimStatusEvaluator,
    ILogger<UpdatePropertyClaimHandler> logger) :
    IRequestHandler<UpdatePropertyClaimCommand, Result<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<UpdatePropertyClaimHandler> _logger = logger;

    public async Task<Result<PropertyClaim>> Handle(UpdatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = await _repository.GetById(command.Id);
        if (claim == null)
        {
            _logger.LogWarning("Attempted to update non-existent property claim: ClaimId={ClaimId}", command.Id);
            return Result.Fail<PropertyClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);
        }

        var oldStatus = claim.Status;
        _mapper.Map(command, claim);

        var allClaims = await _repository.GetByType(claim.Type);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, allClaims);

        var updateResult = await _repository.UpdateClaim(claim);
        if (updateResult.IsFailed)
        {
            _logger.LogError("Failed to update property claim {ClaimId}: {Error}", command.Id, updateResult.Errors[0].Message);
            return Result.Fail<PropertyClaim>(updateResult.Errors[0].Message);
        }

        if (claim is not PropertyClaim propertyClaim)
            return Result.Fail<PropertyClaim>("Claim is not a PropertyClaim.");

        // Log status changes for business monitoring
        if (oldStatus != claim.Status)
        {
            _logger.LogInformation("Property claim status changed: ClaimId={ClaimId}, OldStatus={OldStatus}, NewStatus={NewStatus}", 
                claim.Id, oldStatus, claim.Status);
        }
        else
        {
            _logger.LogInformation("Property claim updated: ClaimId={ClaimId}, Status={Status}", claim.Id, claim.Status);
        }

        return Result.Ok(propertyClaim);
    }
}