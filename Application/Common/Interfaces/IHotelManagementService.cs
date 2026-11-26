using BonVent.Contracts.Guests;
using BonVent.Contracts.Rooms;

namespace Application.Common.Interfaces;

public interface IHotelManagementService
{
    Task<IReadOnlyList<RoomTypeResponse>> GetRoomTypesAsync(CancellationToken cancellationToken);
    Task<RoomTypeResponse> CreateRoomTypeAsync(CreateRoomTypeRequest request, CancellationToken cancellationToken);
    Task<RoomTypeResponse> UpdateRoomTypeAsync(Guid id, UpdateRoomTypeRequest request, CancellationToken cancellationToken);
    Task<RoomResponse> CreateRoomAsync(CreateRoomRequest request, CancellationToken cancellationToken);
    Task<RoomResponse> UpdateRoomAsync(Guid id, UpdateRoomRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminGuestResponse>> GetGuestsAsync(int skip, int take, CancellationToken cancellationToken);
    Task<AdminGuestResponse> GetGuestAsync(Guid id, CancellationToken cancellationToken);
    Task<AdminGuestResponse> UpdateGuestAsync(Guid id, UpdateGuestRequest request, CancellationToken cancellationToken);
    Task DeactivateGuestAsync(Guid id, CancellationToken cancellationToken);
}
