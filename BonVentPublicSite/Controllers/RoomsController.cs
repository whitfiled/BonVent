using BonVent.Contracts.Rooms;
using BonVentPublicSite.Models;
using BonVentPublicSite.Services.Interfaces;
using BonVentPublicSite.Services.Implementation;
using Contracts.Bookings;
using Microsoft.AspNetCore.Mvc;

namespace BonVentPublicSite.Controllers;

public sealed class RoomsController(IRoomService rooms, IReservationClient reservation, AvailabilityNotifications notifications, ILogger<RoomsController> logger) : Controller
{
    public async Task<IActionResult> Index(DateOnly? checkIn, DateOnly? checkOut, int? guests, CancellationToken ct)
    {
        var start = checkIn ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var end = checkOut ?? start.AddDays(1);
        var count = guests is > 0 and <= 20 ? guests.Value : 1;
        var available = Array.Empty<RoomResponse>() as IReadOnlyList<RoomResponse>;
        if (start < DateOnly.FromDateTime(DateTime.Today) || end <= start)
            ModelState.AddModelError(string.Empty, "Choose a current check-in date and a later check-out date.");
        else
        {
            try { available = await rooms.GetRoomsAsync(new RoomAvailabilityRequest(start, end, count), ct); }
            catch (HttpRequestException) { ModelState.AddModelError(string.Empty, "Available rooms could not be loaded. Please try again."); }
        }
        return View(new HomeViewModel { CheckIn = start, CheckOut = end, Guest = count, Rooms = available });
    }

    public async Task<IActionResult> Details(Guid id, DateOnly? checkIn, DateOnly? checkOut, int? guests, CancellationToken cancellationToken)
    {
        RoomResponse? room;
        try { room = await rooms.GetRoomByIdAsync(id, cancellationToken); }
        catch (HttpRequestException)
        {
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("ServiceUnavailable");
        }

        if (room is null)
        {
            return NotFound();
        }

        ViewBag.CheckIn = checkIn;
        ViewBag.CheckOut = checkOut;
        ViewBag.Guests = guests;

        return View(room);
    }

    [HttpGet("Rooms/Reservation")]
    public async Task<IActionResult> Reservations(Guid? roomId, DateOnly? checkIn, DateOnly? checkOut, int? guests, CancellationToken ct)
    {
        var form = new ReservationViewModel
        {
            CheckIn = checkIn ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            CheckOut = checkOut ?? (checkIn ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1))).AddDays(1),
            RoomId = roomId,
            Guests = guests is > 0 and <= 20 ? guests.Value : 1
        };

        if (form.CheckIn < DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError(nameof(form.CheckIn), "Check-in cannot be in the past.");
        if (form.CheckOut <= form.CheckIn)
            ModelState.AddModelError(nameof(form.CheckOut), "Check-out must be after check-in.");
        await LoadRoomsAsync(form, ct);
        return View(form);

    }

    [HttpPost("Rooms/Reservation")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservations(ReservationViewModel form, CancellationToken cancellationToken)
    {
        if (form.CheckIn < DateOnly.FromDateTime(DateTime.Today))
            ModelState.AddModelError(nameof(form.CheckIn), "Check-in cannot be in the past.");
        if (form.CheckOut <= form.CheckIn)
            ModelState.AddModelError(nameof(form.CheckOut), "Check-out must be after check-in.");
        await LoadRoomsAsync(form, cancellationToken);

        if (form.RoomId is Guid selectedRoom && !form.AvailableRooms.Any(room => room.Id == selectedRoom))
            ModelState.AddModelError(nameof(form.RoomId), "This room is no longer available for the selected dates and guests.");

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
            var response = await reservation.CreateAsync(
                new PublicReservationRequest(
                    form.FirstName,
                    form.LastName,
                    form.Email,
                    form.Phone,
                    form.Address,
                    roomId,
                    form.CheckIn,
                    form.CheckOut,
                    form.IdempotencyKey == Guid.Empty ? Guid.NewGuid() : form.IdempotencyKey),
                cancellationToken);

            try { await notifications.NotifyAsync(); }
            catch (Exception exception) { logger.LogWarning(exception, "Unable to send availability notification."); }
            return View("Confirmation", response);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "The reservation request could not be completed.");
            return View(form);
        }
    }

    private async Task LoadRoomsAsync(ReservationViewModel form, CancellationToken cancellationToken)
    {
        if (form.CheckOut <= form.CheckIn || form.CheckIn < DateOnly.FromDateTime(DateTime.Today) || form.Guests is < 1 or > 20)
        {
            return;
        }

        try
        {
            form.AvailableRooms = await rooms.GetRoomsAsync(
                new RoomAvailabilityRequest(
                    form.CheckIn,
                    form.CheckOut,
                    Guests: form.Guests),
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Available rooms could not be loaded.");
        }
    }
}
