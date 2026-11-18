namespace Application.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    Guid GuestId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    Guid IdempotencyKey);
