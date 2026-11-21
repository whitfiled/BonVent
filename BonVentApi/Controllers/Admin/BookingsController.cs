using Application.Bookings.Commands.CreateBooking;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[Tags("Bookings (Staff)")]
internal sealed class BookingsController(BookingHandler handler) : AdminControllerBase
{
    [HttpPost]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingForGuestRequest request, CancellationToken ct)
    {
        var response = await handler.HandleAsync(
            new CreateBookingCommand(request.GuestId, request.RoomId, request.CheckIn, request.CheckOut, request.IdempotencyKey), ct);

        return Created($"/api/v1/manage/bookings/{response.Id}", response);
    }

    [HttpGet]
    [Authorize(Policy = PermissionScopes.ReservationsRead)]
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll([FromServices] IReservationService reservations, CancellationToken cancellationToken)
    {
        return Ok(await reservations.GetForStaffAsync(cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken)
    {
        return Ok(await reservations.CancelAsync(id, null, cancellationToken));
    }
}
