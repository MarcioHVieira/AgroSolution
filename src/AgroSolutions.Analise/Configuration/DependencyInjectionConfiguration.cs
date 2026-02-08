using AgroSolutions.Analise.Application.Interfaces;
using AgroSolutions.Analise.Application.Services;
using AgroSolutions.Analise.Application.Validators;
using AgroSolutions.Analise.Configuration.Settings;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Repositories;
using AgroSolutions.Analise.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AgroSolutions.Analise.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurações (Options Pattern)
        services.Configure<RabbitMQSettings>(configuration.GetSection("RabbitMQ"));
        services.Configure<MotorRegrasSettings>(configuration.GetSection("MotorRegras"));

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CriarAlertaDtoValidator>();

        // API
        services.AddApiConfiguration();

        // Repositories
        services.AddScoped<IAlertaRepository, AlertaRepository>();
        services.AddScoped<IRegraAlertaRepository, RegraAlertaRepository>();

        // Services
        services.AddScoped<IAlertaService, AlertaService>();
        services.AddScoped<IMotorRegrasService, MotorRegrasService>();

        // Memory Cache para o Motor de Regras
        services.AddMemoryCache();

        // RabbitMQ Publisher para Alertas
        services.AddSingleton<IRabbitMQAlertaPublisherService, RabbitMQAlertaPublisherService>();

        // Background Services
        services.AddHostedService<TalhaoSyncConsumerService>();
        services.AddHostedService<RabbitMQAnaliseConsumerService>();

        return services;
    }
}

