using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.Commands;

public record CreatePropertyClaimCommand(
    string Address,
    PropertyDamageType PropertyDamageType,
    decimal EstimatedDamageCost,
    DateTime ReportedDate,
    string Description
) : IRequest<Result<Guid>>;

public record UpdatePropertyClaimCommand(
    Guid Id,
    string Address,
    PropertyDamageType PropertyDamageType,
    decimal EstimatedDamageCost,
    DateTime ReportedDate,
    string Description,
    ClaimStatus? Status
) : IRequest<Result>;

public record DeletePropertyClaimCommand(Guid Id) : IRequest<Result>;
