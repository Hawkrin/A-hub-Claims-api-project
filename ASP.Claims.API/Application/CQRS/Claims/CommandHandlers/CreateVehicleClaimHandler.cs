namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateVehicleClaimHandler(
    IClaimRepository repository, 
    IMapper mapper, 
    IClaimStatusEvaluator claimStatusEvaluator,
    ILogger<CreateVehicleClaimHandler> logger) : 
    IRequestHandler<CreateVehicleClaimCommand, Result<VehicleClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IClaimStatusEvaluator _claimStatusEvaluator = claimStatusEvaluator;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CreateVehicleClaimHandler> _logger = logger;

    public async Task<Result<VehicleClaim>> Handle(CreateVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = _mapper.Map<VehicleClaim>(command);
        claim.Status = _claimStatusEvaluator.Evaluate(claim, null);

        var saveResult = await _repository.Save(claim);
        if (saveResult.IsFailed)
        {
            _logger.LogError("Failed to create vehicle claim: {Error}", saveResult.Errors[0].Message);
            return Result.Fail<VehicleClaim>(saveResult.Errors[0].Message);
        }

        _logger.LogInformation("Vehicle claim created: ClaimId={ClaimId}, RegistrationNumber={RegNumber}, Location={Location}, Status={Status}", 
            claim.Id, command.RegistrationNumber, command.PlaceOfAccident, claim.Status);

        return Result.Ok(claim);
    }
}