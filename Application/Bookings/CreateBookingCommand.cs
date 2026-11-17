namespace Application.Bookings;

internal sealed record CreateBookingCommand(
    Guid GuestId,
    Guid RoomId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate);
