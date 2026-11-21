namespace Contracts.Bookings;

public sealed record PublicReservationRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    string Address,
    Guid RoomId,
    DateOnly CheckIn,
    DateOnly CheckOut,
    Guid IdempotencyKey);
