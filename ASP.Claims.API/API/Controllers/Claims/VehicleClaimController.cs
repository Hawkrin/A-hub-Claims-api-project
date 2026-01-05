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
public class VehicleClaimController(IMediator mediator, IMapper mapper) : ControllerBase
{
    private readonly IMediator _mediator = mediator;
    private readonly IMapper _mapper = mapper;

    /// <summary>
    /// Gets a vehicle claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>The vehicle claim if found; otherwise, 404 Not Found.</returns>
    [Authorize]
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
    [Authorize]
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
    /// <returns>The created vehicle claim object.</returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VehicleClaimDto dto)
    {
        var command = _mapper.Map<CreateVehicleClaimCommand>(dto);

        var res = await _mediator.Send(command);

        if (res.IsFailed)
            return BadRequest(new { message = res.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });

        var createdClaim = res.Value;
        var responseDto = _mapper.Map<VehicleClaimDto>(createdClaim);
        return CreatedAtAction(nameof(GetById), new { id = createdClaim.Id }, responseDto);
    }

    /// <summary>
    /// Updates an existing vehicle claim.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <param name="dto">The updated vehicle claim data.</param>
    /// <returns>The updated vehicle claim object.</returns>
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] VehicleClaimDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = ErrorMessages.ErrorMessage_RouteIdAndDTOIdDoNotMatch });

        var command = _mapper.Map<UpdateVehicleClaimCommand>(dto);

        var result = await _mediator.Send(command);

        if (result.IsFailed)
            return BadRequest(new { message = result.Errors[0]?.Message ?? ErrorMessages.ErrorMessage_UnknownError });

        if (result.Errors.Any(e => e.Message == ErrorMessages.ErrorMessage_ClaimNotFound))
            return NotFound(new { message = result.Errors[0].Message });

        var updatedClaim = result.Value;
        var responseDto = _mapper.Map<VehicleClaimDto>(updatedClaim);
        return Ok(responseDto);
    }

    /// <summary>
    /// Deletes a vehicle claim by its unique identifier.
    /// </summary>
    /// <param name="id">The claim's unique identifier.</param>
    /// <returns>No content if successful; 404 Not Found if claim does not exist.</returns>
    [Authorize]
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