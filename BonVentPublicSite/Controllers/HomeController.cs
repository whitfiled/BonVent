using BonVent.Contracts.Rooms;
using BonVentPublicSite.Models;
using BonVentPublicSite.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BonVentPublicSite.Controllers;

public sealed class HomeController(IRoomService room) : Controller
{
    public async Task<IActionResult> Index(DateOnly? checkIn, DateOnly? checkOut, int? guest, CancellationToken ct)
    {
        var selectedCheckIn = checkIn ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var selectedCheckOut = checkOut ?? selectedCheckIn.AddDays(2);

        var selectedGuest = guest is > 0 ? guest.Value : 1;

        var model = new HomeViewModel
        {
            CheckIn = selectedCheckIn,
            CheckOut = selectedCheckOut,
            Guest = selectedGuest
        };

        if (selectedCheckIn < DateOnly.FromDateTime(DateTime.Today) || selectedCheckOut <= selectedCheckIn)
        {
            ModelState.AddModelError(string.Empty, "Choose a current check-in date and a later check-out date.");
            return View(model);
        }

        try
        {
            model = new HomeViewModel
            {
                CheckIn = selectedCheckIn,
                CheckOut = selectedCheckOut,
                Guest = selectedGuest,

                Rooms = await room.GetRoomsAsync(new RoomAvailabilityRequest(selectedCheckIn, selectedCheckOut, selectedGuest), ct)
            };
        }

        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The Room service is currently unavailable");
        }
        return View(model);
    }
}
