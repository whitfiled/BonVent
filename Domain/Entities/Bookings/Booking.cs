using Domain.Entities.Common;
using Domain.Entities.Guests;
using Domain.Entities.RoomEntity;
using Domain.Entities.Status;

namespace Domain.Entities.Bookings;

internal class Booking : AuditableEntity, ISoftDelete, IAggregateRoot
{
    public Guid GuestId { get; private set; }
    public Guest Guest { get; private set; } = default!;

    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = default!;

    public DateOnly CheckInDate { get; private set; }
    public DateOnly CheckOutDate { get; private set; }
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public Guid IdempotencyKey { get; private set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }

    private Booking() { }

    public static Booking Create(Guid guestId, Room room, DateOnly checkIn, DateOnly checkOut, decimal nightlyRate, Guid idempotencyKey)
    {
        if (checkOut <= checkIn)
            throw new ArgumentException("Check-out must be after check-in.");
        if (room.IsOutOfService)
            throw new InvalidOperationException("Room is out of service.");

        var nights = checkOut.DayNumber - checkIn.DayNumber;

        return new Booking
        {
            GuestId = guestId,
            RoomId = room.Id,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            TotalPrice = nights * nightlyRate,
            Status = BookingStatus.Pending,
            IdempotencyKey = idempotencyKey
        };
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be confirmed.");
        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status is BookingStatus.CheckedIn or BookingStatus.CheckedOut)
            throw new InvalidOperationException("Cannot cancel after check-in.");
        Status = BookingStatus.Cancelled;
    }

    public void CheckIn()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException("Booking must be confirmed first.");
        Status = BookingStatus.CheckedIn;
    }

    public void CheckOut()
    {
        if (Status != BookingStatus.CheckedIn)
            throw new InvalidOperationException("Guest is not checked in.");
        Status = BookingStatus.CheckedOut;
    }

    public void Reject()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be rejected.");
        Status = BookingStatus.Cancelled;
    }
}
