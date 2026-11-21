using Application.Common.Interfaces;
using BonVent.Contracts.Rooms;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

public sealed class RoomsController(IRoomImageService images) : AdminControllerBase
{
    [HttpPost("{roomId:guid}/images")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<RoomImageResponse>> UploadImage(Guid roomId, IFormFile file, [FromForm] string? altText, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
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
}
