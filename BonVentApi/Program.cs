using BonVentApi.Configuration.Extensions;
using BonVentApi.Configuration.Middleware;
using Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseHttpPlane();

using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider, app.Configuration);
}

app.Run();
