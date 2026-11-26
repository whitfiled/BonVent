using Domain.Entities.Bookings;

namespace Domain.IRepositories;

internal interface IBookingRepository
{
    Task<bool> HasOverlapAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken cancellationToken);
    Task<Booking?> GetByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetForGuestAsync(Guid guestId, int skip, int take, CancellationToken cancellationToken);
    Task<IReadOnlyList<Booking>> GetAllAsync(int skip, int take, CancellationToken cancellationToken);
    Task AddAsync(Booking booking, CancellationToken cancellationToken);
}
