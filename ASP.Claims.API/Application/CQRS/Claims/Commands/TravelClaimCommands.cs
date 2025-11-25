using ASP.Claims.API.Domain.Enums;
using FluentResults;
using MediatR;

namespace ASP.Claims.API.Application.CQRS.Claims.Commands;

public record CreateTravelClaimCommand(
    Country Country,
    DateTime StartDate,
    DateTime EndDate,
    IncidentType IncidentType,
    DateTime ReportedDate,
    string Description
) : IRequest<Result<Guid>>;

public record UpdateTravelClaimCommand(
    Guid Id,
    Country Country,
    DateTime StartDate,
    DateTime EndDate,
    IncidentType IncidentType,
    DateTime ReportedDate,
    string Description,
    ClaimStatus? Status
) : IRequest<Result>;

public record DeleteTravelClaimCommand(Guid Id) : IRequest<Result>;