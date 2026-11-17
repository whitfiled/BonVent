using Domain.Entities.Bookings;
using Domain.Entities.RoomEntity;

namespace Domain.IRepositories;

internal interface IBookingRepository
{
    Task<bool> HasOverlapAsync(Guid roomId, DateOnly checkIn, DateOnly checkOut, CancellationToken ct);
    Task<Room?> GetRoomAsync(Guid roomId, CancellationToken ct);
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct);
}
