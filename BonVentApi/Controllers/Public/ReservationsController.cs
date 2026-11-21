using Application.Bookings.Commands.CreatePublicReservation;
using Contracts.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BonVentApi.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/reservations")]
[EnableRateLimiting("PublicReservations")]
[Tags("Reservations (Public)")]
internal sealed class ReservationsController(PublicReservationHandler handler) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(PublicReservationRequest request, CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new PublicReservationCommand(request),
            cancellationToken);

        return Created($"/api/v1/reservations/{response.Id}", response);
    }
}
