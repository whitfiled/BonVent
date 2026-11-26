namespace BonVentApi.Configuration.Middleware;

public static class HttpSecurity
{
    public static WebApplication UseHttpSecurity(this WebApplication app)
    {
        app.Use(async (ctx, next) =>
        {
            ctx.Response.OnStarting(() =>
            {
                var h = ctx.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "no-referrer";
                if (!HttpMethods.IsGet(ctx.Request.Method)
                    || ctx.User.Identity?.IsAuthenticated == true)
                {
                    h["Cache-Control"] = "no-store";
                }
                if (!app.Environment.IsDevelopment())
                    h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
                return Task.CompletedTask;
            });
            await next();

        });

        return app;
    }
}
