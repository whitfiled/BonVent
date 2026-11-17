using Domain.Entities.Bookings;
using Domain.Entities.RoomEntity;
using Domain.Entities.Status;
using Domain.IRepositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class BookingRepository(HotelDbContext context) : IBookingRepository
{
    public Task<bool> HasOverlapAsync(
        Guid roomId,
        DateOnly checkIn,
        DateOnly checkOut,
        CancellationToken cancellationToken)
    {
        return context.Bookings.AnyAsync(
            booking => booking.RoomId == roomId
                && booking.Status != BookingStatus.Cancelled
                && booking.CheckInDate < checkOut
                && booking.CheckOutDate > checkIn,
            cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(booking);
        await context.Bookings.AddAsync(booking, cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Bookings.SingleOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    public Task<Room?> GetRoomAsync(Guid roomId, CancellationToken cancellationToken)
    {
        return context.Set<Room>()
            .Include(room => room.RoomType)
            .SingleOrDefaultAsync(room => room.Id == roomId, cancellationToken);
    }
}
