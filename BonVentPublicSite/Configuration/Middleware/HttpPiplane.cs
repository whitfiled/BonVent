namespace BonVentPublicSite.Configuration.Middleware;

public static class HttpPiplane
{
    public static WebApplication UsePublicSiteMiddlware(this WebApplication app)
    {
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.MapStaticAssets();

        app.UseRouting();

        app.UseHttpSecurity();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

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
