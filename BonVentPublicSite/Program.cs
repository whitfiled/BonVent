using BonVentPublicSite.Configuration.Extensions;
using BonVentPublicSite.Configuration.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPublicSiteService(builder.Configuration);

var app = builder.Build();

app.UsePublicSiteMiddlware();

app.Run();
