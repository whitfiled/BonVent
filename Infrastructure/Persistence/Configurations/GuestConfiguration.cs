using Domain.Entities.Guests;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations;

internal class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Guest> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(150);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.Property(x => x.Address).HasMaxLength(500);


        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
