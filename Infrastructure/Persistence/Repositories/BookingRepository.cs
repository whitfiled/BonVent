using Domain.Entities.Bookings;
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

    public Task<Booking?> GetByIdempotencyKeyAsync(Guid key, CancellationToken cancellationToken)
    {
        return context.Bookings
            .Include(booking => booking.Room)
            .SingleOrDefaultAsync(booking => booking.IdempotencyKey == key, cancellationToken);
    }

    public Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Bookings
            .Include(booking => booking.Room)
            .SingleOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetForGuestAsync(
        Guid guestId,
        CancellationToken cancellationToken)
    {
        return await context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Room)
            .Where(booking => booking.GuestId == guestId)
            .OrderByDescending(booking => booking.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Bookings
            .AsNoTracking()
            .Include(booking => booking.Room)
            .OrderByDescending(booking => booking.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Booking booking, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(booking);
        await context.Bookings.AddAsync(booking, cancellationToken);
    }
}
