using Application.Bookings.Commands.CreateBooking;
using Application.Bookings;
using Application.Bookings.Commands.CreatePublicReservation;
using Application.Rooms.Queries.GetAvailableRooms;
using Application.Common.Interfaces;
using Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<BookingHandler>(serviceProvider => new BookingHandler(
            serviceProvider.GetRequiredService<IBookingRepository>(),
            serviceProvider.GetRequiredService<IRoomRepository>(),
            serviceProvider.GetRequiredService<IGuestRepository>(),
            serviceProvider.GetRequiredService<IUnitOfWork>(),
            serviceProvider.GetRequiredService<IBookingConcurrencyGuard>()));
        services.AddScoped<IGetAvailableRooms, GetAvailableRoomsHandler>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<PublicReservationHandler>(serviceProvider =>
            new PublicReservationHandler(
                serviceProvider.GetRequiredService<IGuestRepository>(),
                serviceProvider.GetRequiredService<BookingHandler>(),
                serviceProvider.GetRequiredService<IUnitOfWork>()));
        return services;
    }
}
