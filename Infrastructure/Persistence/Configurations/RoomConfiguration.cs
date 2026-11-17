using Domain.Entities.RoomEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Number).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Floor).IsRequired();
        builder.Property(x => x.IsAvailable).IsRequired();
        builder.Property(x => x.RoomTypeId).IsRequired();
        // Configure the relationship with RoomType
        builder.HasOne(x => x.RoomType)
               .WithMany()
               .HasForeignKey(x => x.RoomTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property<byte[]>("RowVersion").IsRowVersion();
    }
}
