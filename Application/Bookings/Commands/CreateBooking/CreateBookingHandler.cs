using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Contracts.Bookings;
using Domain.Entities.Bookings;
using Domain.IRepositories;

namespace Application.Bookings.Commands.CreateBooking;

public sealed class CreateBookingHandler
{
    private readonly IBookingRepository _bookings;
    private readonly IRoomRepository _rooms;
    private readonly IGuestRepository _guests;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookingConcurrencyGuard _concurrencyGuard;

    internal CreateBookingHandler(
        IBookingRepository bookings,
        IRoomRepository rooms,
        IGuestRepository guests,
        IUnitOfWork unitOfWork,
        IBookingConcurrencyGuard concurrencyGuard)
    {
        _bookings = bookings;
        _rooms = rooms;
        _guests = guests;
        _unitOfWork = unitOfWork;
        _concurrencyGuard = concurrencyGuard;
    }

    public async Task<BookingResponse> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.IdempotencyKey == Guid.Empty)
        {
            throw new ArgumentException("An idempotency key is required.", nameof(command));
        }

        await using var bookingLock = await _concurrencyGuard.AcquireAsync(
            command.RoomId,
            command.IdempotencyKey,
            cancellationToken);

        var existing = await _bookings.GetByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
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

        var guest = await _guests.GetGuestAsync(command.GuestId, cancellationToken)
            ?? throw new NotFoundException("Guest not found.");
        var room = await _rooms.GetByIdWithTypeAsync(command.RoomId, cancellationToken)
            ?? throw new NotFoundException("Room not found.");

        if (await _bookings.HasOverlapAsync(command.RoomId, command.CheckIn, command.CheckOut, cancellationToken))
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

        await _bookings.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(booking, room.Number);
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
