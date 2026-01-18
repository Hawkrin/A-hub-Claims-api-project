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
public class PropertyClaimController(IMediator mediator, IMapper mapper) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Gets a property claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>The property claim if found; otherwise, 404 Not Found.</returns>
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PropertyClaimDto>> GetById(Guid id)
    {
        var claim = await _mediator.Send(new GetPropertyClaimByIdQuery(id));
        return claim is not null ? Ok(claim) : NotFound();
    }

    /// <summary>
    /// Gets all property claims.
    /// </summary>
    /// <returns>List of property claims.</returns>
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PropertyClaimDto>>> GetAll()
    {
        var claims = await _mediator.Send(new GetAllPropertyClaimsQuery());
        return Ok(claims);
    }

    /// <summary>
    /// Creates a new property claim.
    /// </summary>
    /// <param name="dto">The property claim data.</param>
    /// <returns>The created claim.</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PropertyClaimDto dto)
    {
        var command = _mapper.Map<CreatePropertyClaimCommand>(dto);
        var res = await _mediator.Send(command);

        if (res.IsFailed)
            return BadRequest(res.Errors[0].Message);

        var createdClaim = res.Value;
        var responseDto = _mapper.Map<PropertyClaimDto>(createdClaim);

        return CreatedAtAction(nameof(GetById), new { id = createdClaim.Id }, responseDto);
    }

    /// <summary>
    /// Updates an existing property claim.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <param name="dto">The updated property claim data.</param>
    /// <returns>The created claim; 400 Bad Request if IDs do not match; 404 Not Found if claim does not exist.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PropertyClaimDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = ErrorMessages.ErrorMessage_RouteIdAndDTOIdDoNotMatch });

        var command = _mapper.Map<UpdatePropertyClaimCommand>(dto);
        var result = await _mediator.Send(command);

        if (result.IsFailed)
        {
            if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
                return NotFound(new { message = result.Errors[0].Message });

            return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
        }

        var updatedClaim = result.Value;
        var responseDto = _mapper.Map<PropertyClaimDto>(updatedClaim);
        return Ok(responseDto);
    }

    /// <summary>
    /// Deletes a property claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>No content if successful; 404 Not Found if claim does not exist.</returns>
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeletePropertyClaimCommand(id));

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
    }
}