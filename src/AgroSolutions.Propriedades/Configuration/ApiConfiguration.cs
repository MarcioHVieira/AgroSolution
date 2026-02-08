using AgroSolutions.SharedKernel.Configuration;

namespace AgroSolutions.Propriedades.Configuration;

public static class ApiConfiguration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddStandardApiConfiguration(addApiResponseFilter: true);
        return services;
    }
}
