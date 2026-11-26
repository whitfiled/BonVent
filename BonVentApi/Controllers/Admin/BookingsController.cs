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
    public async Task<ActionResult<IReadOnlyList<BookingResponse>>> GetAll([FromQuery] int skip = 0, [FromQuery] int take = 50, [FromServices] IReservationService reservations = null!, CancellationToken cancellationToken = default)
    {
        if (skip < 0 || take is < 1 or > 100) return BadRequest("take must be between 1 and 100.");
        return Ok(await reservations.GetForStaffAsync(skip, take, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> Cancel(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken)
    {
        return Ok(await reservations.CancelAsync(id, null, cancellationToken));
    }

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> Confirm(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken) =>
        Ok(await reservations.ConfirmAsync(id, cancellationToken));

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> Reject(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken) =>
        Ok(await reservations.RejectAsync(id, cancellationToken));

    [HttpPost("{id:guid}/check-in")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> CheckIn(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken) =>
        Ok(await reservations.CheckInAsync(id, cancellationToken));

    [HttpPost("{id:guid}/check-out")]
    [Authorize(Policy = PermissionScopes.ReservationsWrite)]
    public async Task<ActionResult<BookingResponse>> CheckOut(Guid id, [FromServices] IReservationService reservations, CancellationToken cancellationToken) =>
        Ok(await reservations.CheckOutAsync(id, cancellationToken));
}
