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

    [HttpGet("{roomId:guid}")]
    [OutputCache(PolicyName = "RoomCatalogue")]
    public async Task<ActionResult<RoomResponse>> GetById(Guid roomId, CancellationToken cancellationToken)
    {
        var room = await handler.GetByIdAsync(roomId, cancellationToken);
        return room is null ? NotFound() : Ok(room);
    }

    [HttpGet("{roomId:guid}/images/{imageId:guid}")]
    [OutputCache(PolicyName = "RoomCatalogue")]
    public async Task<IActionResult> GetImage(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        var image = await images.GetAsync(roomId, imageId, cancellationToken);
        return File(image.Content, image.ContentType);
    }

}
