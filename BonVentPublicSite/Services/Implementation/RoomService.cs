using BonVent.Contracts.Rooms;
using BonVentPublicSite.Services.Interfaces;

namespace BonVentPublicSite.Services.Implementation;

public class RoomService(HttpClient httpClient) : IRoomService
{
    public Task<RoomResponse?> GetRoomByIdAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<RoomResponse>> GetRoomsAsync(RoomAvailabilityRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/rooms/available" +
            $"?checkIn={request.CheckIn:yyyy-MM-dd}" +
            $"&checkOut={request.CheckOut:yyyy-MM-dd}" +
            $"&guests={request.Guests}";

        return await httpClient.GetFromJsonAsync<List<RoomResponse>>(query, ct) ?? [];
    }
}
