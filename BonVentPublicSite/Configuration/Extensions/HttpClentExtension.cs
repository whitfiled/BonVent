using BonVentPublicSite.Configuration.Extensions.Options;
using BonVentPublicSite.Services.Implementation;
using BonVentPublicSite.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace BonVentPublicSite.Configuration.Extensions;

public static class HttpClentExtension
{
    public static IServiceCollection AddExternalService(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<ApiOptions>().Bind(config.GetSection(ApiOptions.SectionName))
            .ValidateDataAnnotations().ValidateOnStart();


        services.AddHttpClient<IRoomService, RoomService>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;

                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.Timeout);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

        services.AddHttpClient<IReservationClient, ReservationClient>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<ApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseAddress);
                client.Timeout = TimeSpan.FromSeconds(options.Timeout);
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            });

        return services;
    }
}
