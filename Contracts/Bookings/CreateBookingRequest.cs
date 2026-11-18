namespace Contracts.Bookings;

public sealed record CreateBookingRequest(
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    Guid IdempotencyKey);
