using Application;
using Application.Common.Interfaces;
using BonVentApi.Middleware;
using BonVentApi.Services;
using Infrastructure;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace BonVentApi.Configuration.Extensions;

public static class ApiServiceExtension
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration, WebApplicationBuilder builder)
    {
        var GlobalCacheDurationSeconds = Math.Clamp(configuration.GetValue("GlobalCach:durationSecond", 300), 30, 3600);

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();


        services.AddOutputCache(options => options.AddPolicy("RoomCatalogue", policy => policy
            .Expire(TimeSpan.FromSeconds(GlobalCacheDurationSeconds))
            .SetVaryByQuery("languageCode")));


        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();


        #region Security
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
            options.Limits.MaxRequestBodySize = 1_000_000;
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
        });


        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromDays(365);
            options.IncludeSubDomains = true;
            options.Preload = true;
        });

        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        services.AddCors(options => options.AddPolicy("Clients", policy => policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization")));

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("PublicReservations", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });
        #endregion



        #region Auth

        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration section is missing.");

        if (string.IsNullOrWhiteSpace(jwt.Issuer) || string.IsNullOrWhiteSpace(jwt.Audience) || Encoding.UTF8.GetByteCount(jwt.Key ?? string.Empty) < 32)
        {
            throw new InvalidOperationException("JWT issuer, audience, and a signing key of at least 32 bytes must be configured.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key ?? string.Empty)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(PermissionScopes.ReservationsRead, policy =>
                policy.RequireClaim("scope", PermissionScopes.ReservationsRead))
            .AddPolicy(PermissionScopes.ReservationsWrite, policy =>
                policy.RequireClaim("scope", PermissionScopes.ReservationsWrite))
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
        #endregion

        return services;
    }
}
