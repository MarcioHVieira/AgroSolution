using AgroSolutions.SharedKernel.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configuração centralizada do RabbitMQ para todos os microserviços
/// </summary>
public static class RabbitMQConfiguration
{
    /// <summary>
    /// Adiciona o RabbitMQ Publisher com configurações tipadas
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="exchangeName">Nome do exchange específico do microserviço (opcional, usa configuração se não fornecido)</param>
    public static IServiceCollection AddRabbitMQPublisher(
        this IServiceCollection services,
        IConfiguration configuration,
        string? exchangeName = null)
    {
        // Configura RabbitMQSettings usando Options Pattern
        services.Configure<RabbitMQSettings>(options =>
        {
            var rabbitMQSection = configuration.GetSection("RabbitMQ");
            rabbitMQSection.Bind(options);
            
            // Sobrescreve o ExchangeName se fornecido
            if (!string.IsNullOrWhiteSpace(exchangeName))
            {
                options.ExchangeName = exchangeName;
            }
        });

        // Registra o publisher como Singleton para manter conexão persistente
        services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

        return services;
    }
}
