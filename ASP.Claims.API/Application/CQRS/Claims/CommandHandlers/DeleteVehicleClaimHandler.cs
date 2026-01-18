using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class DeleteVehicleClaimHandler(
    IClaimRepository repository,
    ILogger<DeleteVehicleClaimHandler> logger) :
    IRequestHandler<DeleteVehicleClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;
    private readonly ILogger<DeleteVehicleClaimHandler> _logger = logger;

    public async Task<Result> Handle(DeleteVehicleClaimCommand command, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteClaim(command.Id);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Vehicle claim deleted: ClaimId={ClaimId}", command.Id);
        }
        else
        {
            _logger.LogWarning("Failed to delete vehicle claim: ClaimId={ClaimId}, Error={Error}",
                command.Id, result.Errors.FirstOrDefault()?.Message);
        }

        return result;
    }
}
