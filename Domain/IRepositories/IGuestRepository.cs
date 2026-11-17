using Domain.Entities.Guests;

namespace Domain.IRepositories;

internal interface IGuestRepository
{
    Task CreateGuestAsync(Guest guest, CancellationToken cancellationToken);
    Task<Guest?> GetGuestAsync(Guid guestId, CancellationToken cancellationToken);
}
