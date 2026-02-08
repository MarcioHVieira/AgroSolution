using AgroSolutions.SharedKernel.Configuration;

namespace AgroSolutions.Analise.Configuration;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddStandardApiConfiguration(addApiResponseFilter: true);
        return services;
    }
}