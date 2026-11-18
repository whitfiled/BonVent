using Domain.Entities.RoomEntity;

namespace Domain.IRepositories;

internal interface IRoomRepository
{
    Task<Room?> GetByIdWithTypeAsync(Guid id, CancellationToken cancellationToken);
}
