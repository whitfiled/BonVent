using Application.Common.Interfaces;
using Domain.IRepositories;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<EntityChangesInterceptor>();
        services.AddSingleton<IBookingConcurrencyGuard, BookingConcurrencyGuard>();

        services.AddDbContext<HotelDbContext>((serviceProvider, options) =>
            options
                .UseInMemoryDatabase("BonVent")
                .AddInterceptors(serviceProvider.GetRequiredService<EntityChangesInterceptor>()));

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<HotelDbContext>());
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();

        return services;
    }
}
