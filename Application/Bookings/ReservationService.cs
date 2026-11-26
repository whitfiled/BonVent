using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Domain.Entities.Bookings;
using Domain.IRepositories;

namespace Application.Bookings;

internal sealed class ReservationService(IBookingRepository bookings, IUnitOfWork unitOfWork, IAuditService audit, ICurrentUser currentUser) : IReservationService
{
    public async Task<IReadOnlyList<BookingResponse>> GetForGuestAsync(
        Guid guestId,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var results = await bookings.GetForGuestAsync(guestId, skip, take, cancellationToken);
        return results.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<BookingResponse>> GetForStaffAsync(
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var results = await bookings.GetAllAsync(skip, take, cancellationToken);
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
        await audit.RecordAsync(currentUser.UserId, "booking.cancelled", "booking", booking.Id.ToString(), null, cancellationToken);
        return Map(booking);
    }

    public Task<BookingResponse> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken) => ChangeStatusAsync(bookingId, static booking => booking.Confirm(), cancellationToken);
    public Task<BookingResponse> RejectAsync(Guid bookingId, CancellationToken cancellationToken) => ChangeStatusAsync(bookingId, static booking => booking.Reject(), cancellationToken);
    public Task<BookingResponse> CheckInAsync(Guid bookingId, CancellationToken cancellationToken) => ChangeStatusAsync(bookingId, static booking => booking.CheckIn(), cancellationToken);
    public Task<BookingResponse> CheckOutAsync(Guid bookingId, CancellationToken cancellationToken) => ChangeStatusAsync(bookingId, static booking => booking.CheckOut(), cancellationToken);

    private async Task<BookingResponse> ChangeStatusAsync(Guid bookingId, Action<Booking> change, CancellationToken cancellationToken)
    {
        var booking = await bookings.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Reservation not found.");
        change(booking);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await audit.RecordAsync(currentUser.UserId, $"booking.{booking.Status.ToString().ToLowerInvariant()}", "booking", booking.Id.ToString(), null, cancellationToken);
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
