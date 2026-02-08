using AgroSolutions.Propriedades.Application.Interfaces;
using AgroSolutions.Propriedades.Application.Services;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Repositories;
using AgroSolutions.Propriedades.Infrastructure.Services;
using AgroSolutions.SharedKernel.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AgroSolutions.Propriedades.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // API Configuration (Controllers + Filters)
        services.AddApiConfiguration();

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddFluentValidationAutoValidation();

        // Repositórios
        services.AddScoped<IPropriedadeRepository, PropriedadeRepository>();
        services.AddScoped<ITalhaoRepository, TalhaoRepository>();
        services.AddScoped<ICulturaRepository, CulturaRepository>();
        services.AddScoped<IUsuarioInfoRepository, UsuarioInfoRepository>();

        // Serviços de Aplicação
        services.AddScoped<IPropriedadeService, PropriedadeService>();
        services.AddScoped<ITalhaoService, TalhaoService>();
        services.AddScoped<ICulturaService, CulturaService>();

        // RabbitMQ Publisher genérico do SharedKernel
        services.AddRabbitMQPublisher(configuration, "agrosolutions.propriedades");

        // Background Services (Consumers)
        services.AddHostedService<UsuarioSyncConsumerService>();

        return services;
    }
}



