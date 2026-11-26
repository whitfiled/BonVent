using Application.Bookings.Commands.CreateBooking;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Domain.Entities.Guests;
using Domain.IRepositories;

namespace Application.Bookings.Commands.CreatePublicReservation;

internal sealed class PublicReservationHandler(IGuestRepository guests, BookingHandler bookings, IUnitOfWork unitOfWork)
{
    public async Task<BookingResponse> HandleAsync(PublicReservationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var request = command.Request;

        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Phone)
            || string.IsNullOrWhiteSpace(request.Address))
        {
            throw new ArgumentException("Guest contact details are required.", nameof(request));
        }

        var email = request.Email.Trim();
        var guest = await guests.GetByEmailAsync(email, cancellationToken);

        if (guest is null)
        {
            guest = new Guest(request.FirstName.Trim(), request.LastName.Trim(), email, request.Phone.Trim(), request.Address.Trim());

            await guests.CreateGuestAsync(guest, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await bookings.HandleAsync(new CreateBookingCommand(guest.Id, request.RoomId, request.CheckIn, request.CheckOut, request.IdempotencyKey), cancellationToken);
    }
}
