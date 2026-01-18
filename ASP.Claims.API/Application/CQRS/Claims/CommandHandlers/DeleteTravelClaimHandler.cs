using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class DeletePropertyTravelHandler(
    IClaimRepository repository,
    ILogger<DeletePropertyTravelHandler> logger) : 
    IRequestHandler<DeleteTravelClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly ILogger<DeletePropertyTravelHandler> _logger = logger;

    public async Task<Result> Handle(DeleteTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteClaim(command.Id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Travel claim deleted: ClaimId={ClaimId}", command.Id);
        }
        else
        {
            _logger.LogWarning("Failed to delete travel claim: ClaimId={ClaimId}, Error={Error}", 
                command.Id, result.Errors.FirstOrDefault()?.Message);
        }

        return result;
    }
}