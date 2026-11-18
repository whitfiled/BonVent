using BonVent.Contracts.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
[Tags("Rooms (Public)")]
public sealed class RoomsController : ControllerBase
{
    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> GetAvailable([FromQuery] RoomAvailabilityRequest request, CancellationToken ct)
    {
        return Ok();
    }
}