namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

public class CreateTravelClaimHandler(
    IClaimRepository repository, 
    IMapper mapper,
    ILogger<CreateTravelClaimHandler> logger) : 
    IRequestHandler<CreateTravelClaimCommand, Result<TravelClaim>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CreateTravelClaimHandler> _logger = logger;

    public async Task<Result<TravelClaim>> Handle(CreateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = _mapper.Map<TravelClaim>(command);

        var saveResult = await _repository.Save(claim);
        if (saveResult.IsFailed)
        {
            _logger.LogError("Failed to create travel claim: {Error}", saveResult.Errors[0].Message);
            return Result.Fail<TravelClaim>(saveResult.Errors[0].Message);
        }

        _logger.LogInformation("Travel claim created: ClaimId={ClaimId}, Country={Country}, IncidentType={IncidentType}, TravelPeriod={StartDate} to {EndDate}, Status={Status}", 
            claim.Id, command.Country, command.IncidentType, command.StartDate, command.EndDate, claim.Status);

        return Result.Ok(claim);
    }
}
