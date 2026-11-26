using Scalar.AspNetCore;

namespace BonVentApi.Configuration.Middleware;

public static class HttpPiplane
{
    public static WebApplication UseHttpPlane(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseForwardedHeaders();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseHttpSecurity();

        app.UseCors("Clients");
        app.UseRateLimiter();
        app.UseOutputCache();

        app.UseAuthentication();
        app.UseAuthorization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference().AllowAnonymous();
        }

        app.MapControllers();

        return app;
    }
}
