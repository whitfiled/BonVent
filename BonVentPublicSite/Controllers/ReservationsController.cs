using BonVent.Contracts.Rooms;
using BonVentPublicSite.Models;
using BonVentPublicSite.Services.Interfaces;
using Contracts.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace BonVentPublicSite.Controllers;

public class ReservationsController(IReservationClient reservations, IRoomService rooms) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var form = new PublicReservationForm
        {
            CheckIn = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CheckOut = DateOnly.FromDateTime(DateTime.Today.AddDays(2))
        };

        await LoadRoomsAsync(form, cancellationToken);
        return View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PublicReservationForm form, CancellationToken cancellationToken)
    {
        await LoadRoomsAsync(form, cancellationToken);

        if (!ModelState.IsValid || form.RoomId is not Guid roomId)
        {
            if (form.RoomId is null)
            {
                ModelState.AddModelError(nameof(form.RoomId), "Please select a room.");
            }

            return View(form);
        }

        try
        {
            var response = await reservations.CreateAsync(
                new PublicReservationRequest(
                    form.FirstName,
                    form.LastName,
                    form.Email,
                    form.Phone,
                    form.Address,
                    roomId,
                    form.CheckIn,
                    form.CheckOut,
                    Guid.NewGuid()),
                cancellationToken);

            return View("Confirmation", response);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The reservation request could not be completed.");
            return View(form);
        }
    }

    private async Task LoadRoomsAsync(PublicReservationForm form, CancellationToken cancellationToken)
    {
        if (form.CheckOut <= form.CheckIn)
        {
            return;
        }

        try
        {
            form.AvailableRooms = await rooms.GetRoomsAsync(
                new RoomAvailabilityRequest(
                    form.CheckIn,
                    form.CheckOut,
                    Guests: 1),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Available rooms could not be loaded.");
        }
    }
}
