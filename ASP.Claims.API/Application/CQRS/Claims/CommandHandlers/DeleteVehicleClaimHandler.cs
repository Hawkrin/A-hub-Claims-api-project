using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class DeleteVehicleClaimHandler(IClaimRepository repository) : IRequestHandler<DeleteVehicleClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;

    public async Task<Result> Handle(DeleteVehicleClaimCommand command, CancellationToken cancellationToken)
        => await _repository.DeleteClaim(command.Id);
}
