using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.IngestaoDados.Application.Services;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using AgroSolutions.IngestaoDados.Infrastructure.Repositories;
using AgroSolutions.IngestaoDados.Infrastructure.Services;
using AgroSolutions.SharedKernel.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AgroSolutions.IngestaoDados.Configuration;

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
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ILeituraSensorRepository, LeituraSensorRepository>();

        // Serviços de Aplicação
        services.AddScoped<ISensorService, SensorService>();
        services.AddScoped<ILeituraService, LeituraService>();

        // RabbitMQ Publisher genérico do SharedKernel
        services.AddRabbitMQPublisher(configuration, "agrosolutions.ingestaodados");
        services.AddScoped<IMensageriaService, MensageriaService>();

        return services;
    }
}



