using BonVentPublicSite.Services.Interfaces;
using Contracts.Bookings;

namespace BonVentPublicSite.Services.Implementation;

public sealed class ReservationClient(HttpClient httpClient) : IReservationClient
{
    public async Task<BookingResponse> CreateAsync(PublicReservationRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/reservations",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<BookingResponse>(cancellationToken) ?? throw new InvalidOperationException("The API returned an empty reservation response.");
    }
}
