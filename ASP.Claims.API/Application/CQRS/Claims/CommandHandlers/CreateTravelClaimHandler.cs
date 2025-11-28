namespace ASP.Claims.API.Application.CQRS.Claims.CommandHandlers;

using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.Interfaces;
using ASP.Claims.API.Domain.Entities;
using AutoMapper;
using FluentResults;
using MediatR;

public class CreateTravelClaimHandler(IClaimRepository repository, IMapper mapper) : IRequestHandler<CreateTravelClaimCommand, Result<Guid>>
{
    private readonly IClaimRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<Guid>> Handle(CreateTravelClaimCommand command, CancellationToken cancellationToken)
    {
        var claim = _mapper.Map<TravelClaim>(command);

        var saveResult = await _repository.Save(claim);
        if (saveResult.IsFailed)
            return Result.Fail<Guid>(saveResult.Errors[0].Message);

        return Result.Ok(claim.Id);
    }
}
