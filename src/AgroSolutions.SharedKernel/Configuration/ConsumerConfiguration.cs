using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Extension methods para configuração de RabbitMQ Consumers
/// </summary>
public static class ConsumerConfiguration
{
    /// <summary>
    /// Adiciona configurações de Consumer ao container de DI
    /// </summary>
    public static IServiceCollection AddConsumerSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ConsumerSettings>(configuration.GetSection("ConsumerSettings"));
        return services;
    }
}
