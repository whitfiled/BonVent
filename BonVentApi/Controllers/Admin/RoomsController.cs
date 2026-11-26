using Application.Common.Interfaces;
using BonVent.Contracts.Rooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

public sealed class RoomsController(IRoomImageService images) : AdminControllerBase
{
    [HttpGet("types")]
    public async Task<ActionResult<IReadOnlyList<RoomTypeResponse>>> GetTypes([FromServices] IHotelManagementService hotel, CancellationToken cancellationToken) =>
        Ok(await hotel.GetRoomTypesAsync(cancellationToken));

    [HttpPost("types")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<RoomTypeResponse>> CreateType(CreateRoomTypeRequest request, [FromServices] IHotelManagementService hotel, CancellationToken cancellationToken) =>
        Ok(await hotel.CreateRoomTypeAsync(request, cancellationToken));

    [HttpPut("types/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<RoomTypeResponse>> UpdateType(Guid id, UpdateRoomTypeRequest request, [FromServices] IHotelManagementService hotel, CancellationToken cancellationToken) =>
        Ok(await hotel.UpdateRoomTypeAsync(id, request, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<RoomResponse>> CreateRoom(CreateRoomRequest request, [FromServices] IHotelManagementService hotel, CancellationToken cancellationToken) =>
        Ok(await hotel.CreateRoomAsync(request, cancellationToken));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<RoomResponse>> UpdateRoom(Guid id, UpdateRoomRequest request, [FromServices] IHotelManagementService hotel, CancellationToken cancellationToken) =>
        Ok(await hotel.UpdateRoomAsync(id, request, cancellationToken));

    [HttpPost("{roomId:guid}/images")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<RoomImageResponse>> UploadImage(Guid roomId, IFormFile? file, [FromForm] string? altText, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("An image file is required.");
        }

        await using var stream = file.OpenReadStream();
        var response = await images.AddAsync(
            roomId,
            new RoomImageUpload(stream, file.FileName, file.ContentType, altText),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{roomId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> DeleteImage(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        await images.DeleteAsync(roomId, imageId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{roomId:guid}/images/{imageId:guid}/primary")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> SetPrimaryImage(Guid roomId, Guid imageId, CancellationToken cancellationToken)
    {
        await images.SetPrimaryAsync(roomId, imageId, cancellationToken);
        return NoContent();
    }
}
