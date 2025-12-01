using Asp.Versioning;
using ASP.Claims.API.API.DTOs.Claims;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Resources;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Claims.API.API.Controllers.Claims;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class TravelClaimController(IMediator mediator, IMapper mapper) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Gets a travel claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>The travel claim if found; otherwise, 404 Not Found.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TravelClaimDto>> GetById(Guid id)
    {
        var claim = await _mediator.Send(new GetTravelClaimByIdQuery(id));
        return claim is not null ? Ok(claim) : NotFound();
    }

    /// <summary>
    /// Gets all travel claims.
    /// </summary>
    /// <returns>List of travel claims.</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TravelClaimDto>>> GetAll()
    {
        var claims = await _mediator.Send(new GetAllTravelClaimsQuery());
        return Ok(claims);
    }

    /// <summary>
    /// Creates a new travel claim.
    /// </summary>
    /// <param name="dto">The travel claim data.</param>
    /// <returns>The created claim's ID.</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TravelClaimDto dto)
    {
        var command = _mapper.Map<CreateTravelClaimCommand>(dto);

        var res = await _mediator.Send(command);

        var id = res.ValueOrDefault;
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing travel claim.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <param name="dto">The updated travel claim data.</param>
    /// <returns>No content if successful; 400 Bad Request if IDs do not match; 404 Not Found if claim does not exist.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] TravelClaimDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = ErrorMessages.ErrorMessage_RouteIdAndDTOIdDoNotMatch });

        var command = _mapper.Map<UpdateTravelClaimCommand>(dto);

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
    }


    /// <summary>
    /// Deletes a travel claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>No content if successful; 404 Not Found if claim does not exist.</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteTravelClaimCommand(id));

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
    }
}