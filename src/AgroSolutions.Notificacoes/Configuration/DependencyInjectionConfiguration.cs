using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Application.Services;
using AgroSolutions.Notificacoes.Application.Validators;
using AgroSolutions.Notificacoes.Configuration.Settings;
using AgroSolutions.Notificacoes.Domain.Interfaces;
using AgroSolutions.Notificacoes.Infrastructure.Repositories;
using AgroSolutions.Notificacoes.Infrastructure.Services;
using AgroSolutions.SharedKernel.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AgroSolutions.Notificacoes.Configuration;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurações (Options Pattern)
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Consumer Settings
        services.AddConsumerSettings(configuration);

        // FluentValidation
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CriarNotificacaoDtoValidator>();

        // API
        services.AddApiConfiguration();

        // Repositories
        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();

        // Services
        services.AddScoped<INotificacaoService, NotificacaoService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IProcessadorNotificacoesService, ProcessadorNotificacoesService>();

        // RabbitMQ Publisher genérico do SharedKernel
        services.AddRabbitMQPublisher(configuration, "agrosolutions.notificacoes");

        // Background Services
        services.AddHostedService<RabbitMQNotificacoesConsumerService>();
        services.AddHostedService<AlertaSensorConsumerService>();
        services.AddHostedService<PropriedadeSyncConsumerService>();
        services.AddHostedService<ProcessadorNotificacoesBackgroundService>();

        return services;
    }
}


