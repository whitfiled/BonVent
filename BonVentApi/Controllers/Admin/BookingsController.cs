using Application.Bookings.Commands.CreateBooking;
using Contracts.Bookings;
using Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Tags("Bookings (Staff)")]
public sealed class BookingsController(CreateBookingHandler handler) : AdminControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingForGuestRequest request, CancellationToken ct)
    {
        var response = await handler.HandleAsync(
            new CreateBookingCommand(request.GuestId, request.RoomId, request.CheckIn, request.CheckOut, request.IdempotencyKey), ct);

        return Created($"/api/v1/manage/bookings/{response.Id}", response);
    }
}