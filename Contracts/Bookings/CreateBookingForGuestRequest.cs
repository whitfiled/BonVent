namespace Contracts.Bookings;

public sealed record CreateBookingForGuestRequest(
    Guid GuestId,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    Guid IdempotencyKey);
