using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using ASP.Claims.API.Resources;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class UpdateVehicleClaimHandler(
    IClaimRepository repository, 
    IMapper mapper, 
    IClaimStatusEvaluator claimStatusEvaluator,
    IClaimEventPublisher eventPublisher,
    ILogger<UpdateVehicleClaimHandler> logger) : 
    IRequestHandler<UpdateVehicleClaimCommand, Result<VehicleClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;
    private readonly IClaimEventPublisher _eventPublisher = eventPublisher;
    private readonly ILogger<UpdateVehicleClaimHandler> _logger = logger;

    public async Task<Result<VehicleClaim>> Handle(UpdateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var existingClaim = await _repository.GetById(command.Id);

        if (existingClaim is not VehicleClaim vehicleClaim)
        {
            _logger.LogWarning("Attempted to update non-existent vehicle claim: ClaimId={ClaimId}", command.Id);
            return Result.Fail<VehicleClaim>(ErrorMessages.ErrorMessage_ClaimNotFound);
        }

        var oldStatus = vehicleClaim.Status;
        _mapper.Map(command, vehicleClaim);
        vehicleClaim.Status = _claimStatusEvaluator.Evaluate(vehicleClaim, null);

        var updateResult = await _repository.UpdateClaim(vehicleClaim);
        if (updateResult.IsFailed)
        {
            _logger.LogError("Failed to update vehicle claim {ClaimId}: {Error}", command.Id, updateResult.Errors[0].Message);
            return Result.Fail<VehicleClaim>(updateResult.Errors[0].Message);
        }

        // Publish domain events if status changed
        if (oldStatus != vehicleClaim.Status)
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await _eventPublisher.PublishClaimEventsAsync(vehicleClaim, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Event publishing failed for claim {ClaimId} (non-critical)", vehicleClaim.Id);
                }
            }, cancellationToken);
            
            _logger.LogInformation("Vehicle claim status changed: ClaimId={ClaimId}, OldStatus={OldStatus}, NewStatus={NewStatus}", 
                vehicleClaim.Id, oldStatus, vehicleClaim.Status);
        }
        else
        {
            _logger.LogInformation("Vehicle claim updated: ClaimId={ClaimId}, Status={Status}", vehicleClaim.Id, vehicleClaim.Status);
        }

        return Result.Ok(vehicleClaim);
    }
}
