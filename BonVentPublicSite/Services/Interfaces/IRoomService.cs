using BonVent.Contracts.Rooms;

namespace BonVentPublicSite.Services.Interfaces;

public interface IRoomService
{
    public Task<IReadOnlyList<RoomResponse>> GetRoomsAsync(RoomAvailabilityRequest request, CancellationToken ct = default);
    public Task<RoomResponse?> GetRoomByIdAsync(Guid id, CancellationToken ct = default);
}
