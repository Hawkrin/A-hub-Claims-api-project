using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateTravelClaimHandler(
    IClaimRepository repository, 
    IMapper mapper,
    IClaimEventPublisher eventPublisher,
    ILogger<UpdateTravelClaimHandler> logger) : 
    IRequestHandler<UpdateTravelClaimCommand, Result<TravelClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly IClaimEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<UpdateTravelClaimHandler> _logger = logger;

    public async Task<Result<TravelClaim>> Handle(UpdateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);
        if (existingClaim is not TravelClaim travelClaim)
        {
            _logger.LogWarning("Attempted to update non-existent travel claim: ClaimId={ClaimId}", command.Id);
            return Result.Fail<TravelClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);
        }

        var oldStatus = travelClaim.Status;
        _mapper.Map(command, travelClaim);

        var updateResult = await _repository.UpdateClaim(travelClaim);
        if (updateResult.IsFailed)
        {
            _logger.LogError("Failed to update travel claim {ClaimId}: {Error}", command.Id, updateResult.Errors[0].Message);
            return Result.Fail<TravelClaim>(updateResult.Errors[0].Message);
        }

        // Publish domain events if status changed
        if (oldStatus != travelClaim.Status)
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await _eventPublisher.PublishClaimEventsAsync(travelClaim, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Event publishing failed for claim {ClaimId} (non-critical)", travelClaim.Id);
                }
            }, cancellationToken);
            
            _logger.LogInformation("Travel claim status changed: ClaimId={ClaimId}, OldStatus={OldStatus}, NewStatus={NewStatus}", 
                travelClaim.Id, oldStatus, travelClaim.Status);
        }
        else
        {
            _logger.LogInformation("Travel claim updated: ClaimId={ClaimId}, Status={Status}", travelClaim.Id, travelClaim.Status);
        }

        return Result.Ok(travelClaim);
    }
}
