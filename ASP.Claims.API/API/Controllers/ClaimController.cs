using Asp.Versioning;
using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ClaimController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    /// <summary>
    /// Gets all claims.
    /// </summary>
    /// <returns>List of all claims.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Claim>>> GetAll()
    {
        var claims = await _mediator.Send(new GetAllClaimsQuery());
        return Ok(claims);
    }
}