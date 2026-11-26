using Domain.Entities.Bookings;
using Domain.Entities.RoomEntity;
using Domain.Entities.Status;
using Xunit;

namespace BonVentApi.Tests;

public sealed class BookingTests
{
    [Fact]
    public void Create_calculates_total_and_starts_pending()
    {
        var type = new RoomType("Standard", 2, 120m);
        var room = new Room("101", 1, type.Id);
        var booking = Booking.Create(Guid.NewGuid(), room, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 4), 120m, Guid.NewGuid());

        Assert.Equal(360m, booking.TotalPrice);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Lifecycle_requires_confirmation_before_check_in()
    {
        var type = new RoomType("Standard", 2, 120m);
        var room = new Room("101", 1, type.Id);
        var booking = Booking.Create(Guid.NewGuid(), room, new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 2), 120m, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => booking.CheckIn());
        booking.Confirm();
        booking.CheckIn();
        booking.CheckOut();
        Assert.Equal(BookingStatus.CheckedOut, booking.Status);
    }
}
