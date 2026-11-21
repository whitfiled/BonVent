using Domain.Entities.RoomEntity;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Identity;

public static class HotelDataSeeder
{
    public static async Task SeedRoomsAsync(IServiceProvider serviceProvider, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (!configuration.GetValue<bool>("Seed:RoomsEnabled"))
        {
            return;
        }

        var db = serviceProvider.GetRequiredService<HotelDbContext>();
        if (await db.RoomTypes.AnyAsync(cancellationToken))
        {
            return;
        }

        var standard = new RoomType("Standard", 2, 120m);
        var family = new RoomType("Family", 4, 220m);
        db.RoomTypes.AddRange(standard, family);
        db.Rooms.AddRange(
            new Room("101", 1, standard.Id),
            new Room("102", 1, standard.Id),
            new Room("201", 2, family.Id));

        await db.SaveChangesAsync(cancellationToken);
    }
}
