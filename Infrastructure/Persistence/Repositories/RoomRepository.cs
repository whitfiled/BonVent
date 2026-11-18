using Domain.Entities.RoomEntity;
using Domain.IRepositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class RoomRepository(HotelDbContext context) : IRoomRepository
{
    public Task<Room?> GetByIdWithTypeAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Rooms
            .Include(room => room.RoomType)
            .SingleOrDefaultAsync(room => room.Id == id, cancellationToken);
    }
}
