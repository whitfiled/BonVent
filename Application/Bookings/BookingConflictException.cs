namespace Application.Bookings;

internal sealed class BookingConflictException(string message) : Exception(message);
