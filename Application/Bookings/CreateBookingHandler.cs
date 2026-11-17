using Application.Common.Interfaces;
using Domain.Entities.Bookings;
using Domain.IRepositories;

namespace Application.Bookings;

internal sealed class CreateBookingHandler(
    IBookingRepository bookingRepository,
    IGuestRepository guestRepository,
    IUnitOfWork unitOfWork,
    IBookingConcurrencyGuard concurrencyGuard)
{
    public async Task<Guid> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.CheckOutDate <= command.CheckInDate)
        {
            throw new ArgumentException("Check-out must be after check-in.", nameof(command));
        }

        var guest = await guestRepository.GetGuestAsync(command.GuestId, cancellationToken);
        if (guest is null)
        {
            throw new KeyNotFoundException($"Guest '{command.GuestId}' was not found.");
        }

        var room = await bookingRepository.GetRoomAsync(command.RoomId, cancellationToken);
        if (room is null)
        {
            throw new KeyNotFoundException($"Room '{command.RoomId}' was not found.");
        }

        await using var bookingLock = await concurrencyGuard.AcquireAsync(command.RoomId, cancellationToken);

        if (await bookingRepository.HasOverlapAsync(
                command.RoomId,
                command.CheckInDate,
                command.CheckOutDate,
                cancellationToken))
        {
            throw new BookingConflictException("The room is already booked for the requested dates.");
        }

        var booking = Booking.Create(
            guest.Id,
            room,
            command.CheckInDate,
            command.CheckOutDate,
            room.RoomType.BaseRate);

        await bookingRepository.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return booking.Id;
    }
}
