using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

public class DeletePropertyTravelHandler(IClaimRepository repository) : IRequestHandler<DeleteTravelClaimCommand, Result>
{
    private readonly IClaimRepository _repository = repository;

    public async Task<Result> Handle(DeleteTravelClaimCommand command, CancellationToken cancellationToken)
        => await _repository.DeleteClaim(command.Id);
}