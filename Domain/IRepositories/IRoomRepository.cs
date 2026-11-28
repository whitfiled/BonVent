using Domain.Entities.RoomEntity;

namespace Domain.IRepositories;

internal interface IRoomRepository
{
    Task<IReadOnlyList<Room>> GetAvailableAsync(DateOnly checkIn, DateOnly checkOut, int? guests, Guid? roomTypeId, CancellationToken cancellationToken);

    Task<Room?> GetByIdWithTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<Room?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);
}
