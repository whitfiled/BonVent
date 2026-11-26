using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Domain.Entities.Bookings;
using Domain.IRepositories;

namespace Application.Bookings.Commands.CreateBooking;

internal sealed class BookingHandler(IBookingRepository bookings, IRoomRepository rooms, IGuestRepository guests, IUnitOfWork unitOfWork, IBookingConcurrencyGuard concurrencyGuard)
{
    public async Task<BookingResponse> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.IdempotencyKey == Guid.Empty)
        {
            throw new ArgumentException("An idempotency key is required.", nameof(command));
        }

        await using var bookingLock = await concurrencyGuard.AcquireAsync(
            command.RoomId,
            command.IdempotencyKey,
            cancellationToken);

        return await unitOfWork.ExecuteInSerializableTransactionAsync(async () =>
        {
        var existing = await bookings.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);

        if (existing is not null)
        {
            if (existing.GuestId != command.GuestId)
            {
                throw new ConflictException("Idempotency key already used.");
            }

            return Map(existing, existing.Room.Number);
        }

        if (command.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Check-in cannot be in the past.", nameof(command));
        }

        var guest = await guests.GetGuestAsync(command.GuestId, cancellationToken) ?? throw new NotFoundException("Guest not found.");

        var room = await rooms.GetByIdWithTypeAsync(command.RoomId, cancellationToken) ?? throw new NotFoundException("Room not found.");

        if (await bookings.HasOverlapAsync(command.RoomId, command.CheckIn, command.CheckOut, cancellationToken))
        {
            throw new ConflictException("Room is not available for those dates.");
        }

        var booking = Booking.Create(
            guest.Id,
            room,
            command.CheckIn,
            command.CheckOut,
            room.RoomType.BaseRate,
            command.IdempotencyKey);

        await bookings.AddAsync(booking, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(booking, room.Number);
        }, cancellationToken);
    }

    private static BookingResponse Map(Booking booking, string roomNumber) => new(
        booking.Id,
        booking.GuestId,
        booking.RoomId,
        roomNumber,
        booking.CheckInDate,
        booking.CheckOutDate,
        booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber,
        booking.TotalPrice,
        booking.Status.ToString());
}
