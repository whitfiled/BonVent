using Application.Common.Interfaces;
using BonVent.Contracts.Guests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Tags("Guests (Staff)")]
public sealed class GuestsController(IHotelManagementService hotel) : AdminControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminGuestResponse>>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        if (skip < 0 || take is < 1 or > 100) return BadRequest("take must be between 1 and 100.");
        return Ok(await hotel.GetGuestsAsync(skip, take, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminGuestResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await hotel.GetGuestAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<AdminGuestResponse>> Update(Guid id, UpdateGuestRequest request, CancellationToken cancellationToken) =>
        Ok(await hotel.UpdateGuestAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await hotel.DeactivateGuestAsync(id, cancellationToken);
        return NoContent();
    }
}
