using BonVent.Contracts.Rooms;
using BonVentPublicSite.Services.Interfaces;
using System.Net;

namespace BonVentPublicSite.Services.Implementation;

public class RoomService(HttpClient httpClient) : IRoomService
{
    public async Task<RoomResponse?> GetRoomByIdAsync(Guid id, CancellationToken ct = default)
    {
        using var response = await httpClient.GetAsync($"api/v1/rooms/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var room = await response.Content.ReadFromJsonAsync<RoomResponse>(cancellationToken: ct);
        return room is null ? null : NormalizeImages(room);
    }

    public async Task<IReadOnlyList<RoomResponse>> GetRoomsAsync(RoomAvailabilityRequest request, CancellationToken ct = default)
    {
        var query =
            $"api/v1/rooms/available" +
            $"?checkIn={request.CheckIn:yyyy-MM-dd}" +
            $"&checkOut={request.CheckOut:yyyy-MM-dd}" +
            $"&guests={request.Guests}";

        var rooms = await httpClient.GetFromJsonAsync<List<RoomResponse>>(query, ct) ?? [];
        return rooms.Select(NormalizeImages).ToArray();
    }

    private RoomResponse NormalizeImages(RoomResponse room) => room with
    {
        Images = room.Images.Select(image => image with
        {
            Url = string.IsNullOrWhiteSpace(image.Url) ||
                  (Uri.TryCreate(image.Url, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                ? image.Url
                : new Uri(httpClient.BaseAddress!, image.Url.TrimStart('/')).ToString()
        }).ToArray()
    };
}
