using Application.Common.Interfaces;
using Application.Rooms.Queries.GetAvailableRooms;
using BonVent.Contracts.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BonVentApi.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
[Tags("Rooms (Public)")]
[OutputCache(PolicyName = "RoomCatalogue")]
public sealed class RoomsController(IGetAvailableRooms handler, IRoomImageService images) : ControllerBase
{
    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<RoomResponse>>> GetAvailable([FromQuery] RoomAvailabilityRequest request, CancellationToken ct)
    {
        var rooms = await handler.HandleAsync(
            new GetAvailableRoomsQuery(
                request.CheckIn,
                request.CheckOut,
                request.Guests,
                request.RoomTypeId),
            ct);

        return Ok(rooms);
    }

    [HttpGet("{roomId:guid}/images/{imageId:guid}")]
    public async Task<IActionResult> GetImage(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await images.GetAsync(roomId, imageId, cancellationToken);
        return File(image.Content, image.ContentType);
    }

}
