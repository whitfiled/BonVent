namespace BonVentPublicSite.Configuration.Middleware;

public static class HttpPiplane
{
    public static WebApplication UsePublicSiteMiddlware(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseWebSockets();

        app.UseStaticFiles();
        app.MapStaticAssets();

        app.UseRouting();

        app.UseHttpSecurity();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        app.Map("/live/availability", async context =>
            await context.RequestServices.GetRequiredService<BonVentPublicSite.Services.Implementation.AvailabilityNotifications>().AcceptAsync(context));

        app.MapControllerRoute(
            name: "Areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        return app;
    }
}
