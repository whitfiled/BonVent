namespace BonVentPublicSite.Configuration.Extensions;

public static class PublicServiceExtension
{
    public static IServiceCollection AddPublicSiteService(this IServiceCollection services, IConfiguration config)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddAuthentication();
        services.AddAuthorization();
        services.AddSingleton<BonVentPublicSite.Services.Implementation.AvailabilityNotifications>();
        services.AddExternalService(config);


        return services;
    }
}
