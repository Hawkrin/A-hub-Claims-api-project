using Asp.Versioning;
using ASP.Claims.API.API.DTOs;
using ASP.Claims.API.Application.CQRS.Claims.Commands;
using ASP.Claims.API.Application.CQRS.Claims.Queries;
using ASP.Claims.API.Resources;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Claims.API.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class VehicleClaimController(IMediator mediator, IMapper mapper) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Gets a vehicle claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>The vehicle claim if found; otherwise, 404 Not Found.</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleClaimDto>> GetById(Guid id)
    {
        var claim = await _mediator.Send(new GetVehicleClaimByIdQuery(id));
        return claim is not null ? Ok(claim) : NotFound();
    }

    /// <summary>
    /// Gets all vehicle claims.
    /// </summary>
    /// <returns>List of vehicle claims.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleClaimDto>>> GetAll()
    {
        var claims = await _mediator.Send(new GetAllVehicleClaimsQuery());
        return Ok(claims);
    }

    /// <summary>
    /// Creates a new vehicle claim.
    /// </summary>
    /// <param name="dto">The vehicle claim data.</param>
    /// <returns>The created claim's ID.</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleClaimDto dto)
    {
        var command = _mapper.Map<CreateVehicleClaimCommand>(dto);

        var res = await _mediator.Send(command);
        var id = res.ValueOrDefault;

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing vehicle claim.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <param name="dto">The updated vehicle claim data.</param>
    /// <returns>No content if successful; 400 Bad Request if IDs do not match; 404 Not Found if claim does not exist.</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleClaimDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = ErrorMessages.ErrorMessage_RouteIdAndDTOIdDoNotMatch });

        var command = _mapper.Map<UpdateVehicleClaimCommand>(dto);

        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
    }

    /// <summary>
    /// Deletes a vehicle claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>No content if successful; 404 Not Found if claim does not exist.</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteVehicleClaimCommand(id));

        if (result.IsSuccess)
            return NoContent();

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });
    }
}