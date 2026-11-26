using Application.Bookings.Commands.CreateBooking;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Guest;

[Tags("Bookings (Guest)")]
internal sealed class BookingsController(ICurrentUser currentUser, BookingHandler handler) : GuestControllerBase
{
    [HttpPost]
    [ProducesResponseType<BookingResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request, CancellationToken ct)
    {
        if (currentUser.GuestId is not Guid guestId) return Forbid();

        var response = await handler.HandleAsync(new CreateBookingCommand(guestId, request.RoomId, request.CheckIn, request.CheckOut, request.IdempotencyKey), ct);

        return Created($"/api/v1/bookings/{response.Id}", response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetMine([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromServices] IReservationService reservations = null!, CancellationToken cancellationToken = default)
    {
        if (currentUser.GuestId is not Guid guestId) return Forbid();

        if (skip < 0 || take is < 1 or > 100) return BadRequest("take must be between 1 and 100.");
        return Ok(await reservations.GetForGuestAsync(guestId, skip, take, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken)
    {
        if (currentUser.GuestId is not Guid guestId) return Forbid();

        return Ok(await reservations.CancelAsync(id, guestId, cancellationToken));
    }
}
