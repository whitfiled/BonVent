namespace Contracts.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid GuestId,
    Guid RoomId,
    string RoomNumber,
    DateOnly CheckIn,
    DateOnly CheckOut,
    int Nights,
    decimal TotalPrice,
    string Status);
