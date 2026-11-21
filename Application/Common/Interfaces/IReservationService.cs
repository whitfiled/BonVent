using Contracts.Bookings;

namespace Application.Common.Interfaces;

public interface IReservationService
{
    Task<IReadOnlyList<BookingResponse>> GetForGuestAsync(Guid guestId, CancellationToken cancellationToken);

    Task<IReadOnlyList<BookingResponse>> GetForStaffAsync(CancellationToken cancellationToken);

    Task<BookingResponse> CancelAsync(Guid bookingId, Guid? guestId, CancellationToken cancellationToken);
}
