using Domain.Entities.Bookings;

namespace Domain.IRepositories;

internal interface IBookingRepository
{
    Task<bool> HasOverlapAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken);
    Task<Booking?> GetByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
}
