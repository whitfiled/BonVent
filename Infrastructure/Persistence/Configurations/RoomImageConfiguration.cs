using Domain.Entities.RoomEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

internal sealed class RoomImageConfiguration : IEntityTypeConfiguration<RoomImage>
{
    public void Configure(EntityTypeBuilder<RoomImage> builder)
    {
        builder.HasKey(image => image.Id);
        builder.Property(image => image.Id).ValueGeneratedNever();
        builder.Property(image => image.BlobName).IsRequired().HasMaxLength(500);
        builder.Property(image => image.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(image => image.AltText).HasMaxLength(250);
        builder.HasIndex(image => new { image.RoomId, image.SortOrder });
    }
}
