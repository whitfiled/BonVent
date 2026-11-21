using Domain.Entities.Guests;
using Domain.IRepositories;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal class GuestRepository(HotelDbContext context) : IGuestRepository
{

    public async Task CreateGuestAsync(Guest guest, CancellationToken cancellationToken)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(guest);
            await context.Guests.AddAsync(guest, cancellationToken);
        }
        catch (Exception ex)
        {

            throw new Exception("An error occurred while creating the guest.", ex);
        }
    }

    public async Task<Guest?> GetGuestAsync(Guid guestId, CancellationToken cancellationToken)
    {
        try
        {

            var guest = await context.Guests.SingleOrDefaultAsync(guest => guest.Id == guestId, cancellationToken);
            return guest;

        }

        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving the guest.", ex);
        }
    }

    public Task<Guest?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return context.Guests.SingleOrDefaultAsync(
            guest => guest.Email == email,
            cancellationToken);
    }
}
