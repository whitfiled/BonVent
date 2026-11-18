using Domain.Entities.Bookings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TotalPrice).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(x => x.Guest)
               .WithMany()
               .HasForeignKey(x => x.GuestId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Room)
               .WithMany()
               .HasForeignKey(x => x.RoomId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RoomId, x.CheckInDate, x.CheckOutDate });
        builder.HasIndex(x => x.IdempotencyKey).IsUnique();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
