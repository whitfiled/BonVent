using Domain.Entities.RoomEntity;
using Domain.Entities.Status;
using Domain.IRepositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class RoomRepository(HotelDbContext context) : IRoomRepository
{
    public async Task<IReadOnlyList<Room>> GetAvailableAsync(
        DateOnly checkIn,
        DateOnly checkOut,
        int? guests,
        Guid? roomTypeId,
        CancellationToken cancellationToken)
    {
        return await context.Rooms
            .AsNoTracking()
            .Include(room => room.RoomType)
            .Include(room => room.Images)
            .Where(room =>
                !room.IsOutOfService
                && (roomTypeId == null || room.RoomTypeId == roomTypeId)
                && (guests == null || room.RoomType.Capacity >= guests)
                && !context.Bookings.Any(booking =>
                    booking.RoomId == room.Id
                    && booking.Status != BookingStatus.Cancelled
                    && booking.CheckInDate < checkOut
                    && booking.CheckOutDate > checkIn))
            .OrderBy(room => room.RoomType.BaseRate)
            .ThenBy(room => room.Number)
            .ToListAsync(cancellationToken);
    }

    public Task<Room?> GetDetailsAsync(Guid id, CancellationToken cancellationToken) =>
        context.Rooms.AsNoTracking().Include(room => room.RoomType).Include(room => room.Images)
            .SingleOrDefaultAsync(room => room.Id == id, cancellationToken);

    public Task<Room?> GetByIdWithTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Rooms
            .Include(room => room.RoomType)
            .SingleOrDefaultAsync(room => room.Id == id, cancellationToken);
    }
}
