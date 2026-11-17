using Application.Common.Interfaces;
using Domain.Entities.Bookings;
using Domain.Entities.Guests;
using Domain.Entities.RoomEntity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

internal class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options), IUnitOfWork
{

    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
