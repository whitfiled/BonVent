using Application.Bookings.Commands.CreateBooking;
using Application.Common.Interfaces;
using Domain.IRepositories;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateBookingHandler>(serviceProvider => new CreateBookingHandler(
            serviceProvider.GetRequiredService<IBookingRepository>(),
            serviceProvider.GetRequiredService<IRoomRepository>(),
            serviceProvider.GetRequiredService<IGuestRepository>(),
            serviceProvider.GetRequiredService<IUnitOfWork>(),
            serviceProvider.GetRequiredService<IBookingConcurrencyGuard>()));
        return services;
    }
}
