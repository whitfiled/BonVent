using Contracts.Bookings;

namespace Application.Common.Interfaces;

public interface IReservationService
{
    Task<IReadOnlyList<BookingResponse>> GetForGuestAsync(Guid guestId, int skip, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingResponse>> GetForStaffAsync(int skip, int take, CancellationToken cancellationToken);

    Task<BookingResponse> CancelAsync(Guid bookingId, Guid? guestId, CancellationToken cancellationToken);
    Task<BookingResponse> ConfirmAsync(Guid bookingId, CancellationToken cancellationToken);
    Task<BookingResponse> RejectAsync(Guid bookingId, CancellationToken cancellationToken);
    Task<BookingResponse> CheckInAsync(Guid bookingId, CancellationToken cancellationToken);
    Task<BookingResponse> CheckOutAsync(Guid bookingId, CancellationToken cancellationToken);
}
