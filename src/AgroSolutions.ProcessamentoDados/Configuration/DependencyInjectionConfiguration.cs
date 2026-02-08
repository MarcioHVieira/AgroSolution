using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.ProcessamentoDados.Application.Services;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using AgroSolutions.ProcessamentoDados.Infrastructure.Repositories;
using AgroSolutions.ProcessamentoDados.Infrastructure.Services;
using AgroSolutions.SharedKernel.Configuration;

namespace AgroSolutions.ProcessamentoDados.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // API Configuration
        services.AddApiConfiguration();

        // Repositórios
        services.AddScoped<ILeituraProcessadaRepository, LeituraProcessadaRepository>();
        services.AddScoped<IAgregacaoDadosRepository, AgregacaoDadosRepository>();

        // Serviços de Aplicação
        services.AddScoped<IProcessamentoService, ProcessamentoService>();
        services.AddScoped<IAgregacaoService, AgregacaoService>();

        // RabbitMQ Publisher genérico do SharedKernel
        services.AddRabbitMQPublisher(configuration, "agrosolutions-exchange");

        // RabbitMQ Consumer (Background Service)
        services.AddHostedService<RabbitMQConsumerService>();

        return services;
    }
}
