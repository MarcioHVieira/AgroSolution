using AgroSolutions.Sensores.Services;
using AgroSolutions.SharedKernel.Configuration;

namespace AgroSolutions.Sensores.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // API Configuration (Controllers + JSON)
        services.AddStandardApiConfiguration(addApiResponseFilter: false);

        // Configurações
        services.Configure<SimuladorSettings>(configuration.GetSection("Simulador"));

        // HttpClient configurado
        services.AddHttpClient<IIngestaoApiClient, IngestaoApiClient>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SimuladorSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.IngestaoApi.TimeoutSeconds);
        });

        // Serviços
        services.AddScoped<ISimuladorService, SimuladorService>();

        return services;
    }
}
