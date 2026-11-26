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
        services.Configure<BootstrapOptions>(configuration.GetSection("Bootstrap"));
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 10;
            options.User.RequireUniqueEmail = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AuthDbContext>();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHostedService<EmailOutboxDispatcher>();
        services.AddHostedService<IdentityRoleInitializer>();
        services.AddSingleton<RefreshTokenConcurrencyGuard>();
        services.AddScoped<EntityChangesInterceptor>();

        services.AddSingleton<IBookingConcurrencyGuard, BookingConcurrencyGuard>();

        var hotelConnection = configuration.GetConnectionString("HotelDatabase") ?? throw new InvalidOperationException("ConnectionStrings:HotelDatabase is required.");
        var identityConnection = configuration.GetConnectionString("IdentityDatabase") ?? throw new InvalidOperationException("ConnectionStrings:IdentityDatabase is required.");

        services.AddDbContext<HotelDbContext>((serviceProvider, options) => options
            .UseMySQL(hotelConnection)
            .AddInterceptors(serviceProvider.GetRequiredService<EntityChangesInterceptor>()));

        services.AddDbContext<AuthDbContext>(options => options
            .UseMySQL(identityConnection));

        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<HotelDbContext>());

        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IHotelManagementService, HotelManagementService>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IRoomImageService, AzureBlobRoomImageService>();

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck<HotelDbContext>>("hotel-database")
            .AddCheck<DatabaseHealthCheck<AuthDbContext>>("identity-database");

        return services;
    }
}
