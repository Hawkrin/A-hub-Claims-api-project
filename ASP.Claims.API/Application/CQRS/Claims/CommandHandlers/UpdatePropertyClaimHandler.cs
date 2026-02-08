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
    IClaimEventPublisher eventPublisher,
    ILogger<UpdatePropertyClaimHandler> logger) : 
    IRequestHandler<UpdatePropertyClaimCommand, Result<PropertyClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;
    private readonly IClaimEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<UpdatePropertyClaimHandler> _logger = logger;

    public async Task<Result<PropertyClaim>> Handle(UpdatePropertyClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not PropertyClaim propertyClaim)
        {
            _logger.LogWarning("Attempted to update non-existent property claim: ClaimId={ClaimId}", command.Id);
            return Result.Fail<PropertyClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);
        }

        var oldStatus = propertyClaim.Status;
        _mapper.Map(command, propertyClaim);

        var allClaims = await _repository.GetByType(propertyClaim.Type);
        propertyClaim.Status = _claimStatusEvaluator.Evaluate(propertyClaim, allClaims);

        var updateResult = await _repository.UpdateClaim(propertyClaim);
        if (updateResult.IsFailed)
        {
            _logger.LogError("Failed to update property claim {ClaimId}: {Error}", command.Id, updateResult.Errors[0].Message);
            return Result.Fail<PropertyClaim>(updateResult.Errors[0].Message);
        }

        // Publish domain events if status changed
        if (oldStatus != propertyClaim.Status)
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await _eventPublisher.PublishClaimEventsAsync(propertyClaim, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Event publishing failed for claim {ClaimId} (non-critical)", propertyClaim.Id);
                }
            }, cancellationToken);
            
            _logger.LogInformation("Property claim status changed: ClaimId={ClaimId}, OldStatus={OldStatus}, NewStatus={NewStatus}", 
                propertyClaim.Id, oldStatus, propertyClaim.Status);
        }
        else
        {
            _logger.LogInformation("Property claim updated: ClaimId={ClaimId}, Status={Status}", propertyClaim.Id, propertyClaim.Status);
        }

        return Result.Ok(propertyClaim);
    }
}
