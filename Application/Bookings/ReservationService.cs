using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Domain.Entities.Bookings;
using Domain.IRepositories;

namespace Application.Bookings;

internal sealed class ReservationService(IBookingRepository bookings, IUnitOfWork unitOfWork) : IReservationService
{
    public async Task<IReadOnlyList<BookingResponse>> GetForGuestAsync(
        Guid guestId,
        CancellationToken cancellationToken)
    {
        var results = await bookings.GetForGuestAsync(guestId, cancellationToken);
        return results.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<BookingResponse>> GetForStaffAsync(
        CancellationToken cancellationToken)
    {
        var results = await bookings.GetAllAsync(cancellationToken);
        return results.Select(Map).ToArray();
    }

    public async Task<BookingResponse> CancelAsync(
        Guid bookingId,
        Guid? guestId,
        CancellationToken cancellationToken)
    {
        var booking = await bookings.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Reservation not found.");

        if (guestId is Guid ownerId && booking.GuestId != ownerId)
        {
            throw new UnauthorizedAccessException("You cannot modify this reservation.");
        }

        booking.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(booking);
    }

    private static BookingResponse Map(Booking booking) => new(
        booking.Id,
        booking.GuestId,
        booking.RoomId,
        booking.Room.Number,
        booking.CheckInDate,
        booking.CheckOutDate,
        booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber,
        booking.TotalPrice,
        booking.Status.ToString());
}
