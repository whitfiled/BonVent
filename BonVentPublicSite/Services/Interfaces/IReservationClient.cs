using Contracts.Bookings;

namespace BonVentPublicSite.Services.Interfaces;

public interface IReservationClient
{
    Task<BookingResponse> CreateAsync(PublicReservationRequest request, CancellationToken cancellationToken = default);
}
