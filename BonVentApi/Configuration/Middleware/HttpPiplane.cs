using Scalar.AspNetCore;

namespace BonVentApi.Configuration.Middleware;

public static class HttpPiplane
{
    public static WebApplication UseHttpPlane(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference().AllowAnonymous();
        }

        app.UseExceptionHandler();
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();


        return app;
    }
}
