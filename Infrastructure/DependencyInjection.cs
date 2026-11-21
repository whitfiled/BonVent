using Application.Common.Interfaces;
using Domain.IRepositories;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 10;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<HotelDbContext>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<RefreshTokenConcurrencyGuard>();
        services.AddScoped<EntityChangesInterceptor>();

        services.AddSingleton<IBookingConcurrencyGuard, BookingConcurrencyGuard>();

        services.AddDbContext<HotelDbContext>((serviceProvider, options) =>
            options
                .UseInMemoryDatabase("BonVent")
                .AddInterceptors(serviceProvider.GetRequiredService<EntityChangesInterceptor>()));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<HotelDbContext>());

        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomImageService, AzureBlobRoomImageService>();

        return services;
    }
}
