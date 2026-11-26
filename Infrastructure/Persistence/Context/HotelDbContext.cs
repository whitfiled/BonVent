using System.Data;
using Application.Common.Interfaces;
using Domain.Entities.Bookings;
using Domain.Entities.Guests;
using Domain.Entities.RoomEntity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class HotelDbContext(DbContextOptions<HotelDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<RoomType> RoomTypes => Set<RoomType>();
    internal DbSet<Room> Rooms => Set<Room>();
    internal DbSet<Guest> Guests => Set<Guest>();
    internal DbSet<Booking> Bookings => Set<Booking>();

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);

        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var result = await action();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
