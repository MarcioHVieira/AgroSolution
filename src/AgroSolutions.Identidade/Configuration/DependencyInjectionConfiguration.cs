using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Application.Services;
using AgroSolutions.Identidade.Configuration.Settings;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Repositories;
using AgroSolutions.Identidade.Infrastructure.Security;
using AgroSolutions.Identidade.Infrastructure.Services;
using AgroSolutions.SharedKernel.Configuration;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace AgroSolutions.Identidade.Configuration;

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

        // Configurações tipadas
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        // Gerenciador de chaves RSA (Singleton para evitar regeneração)
        services.AddSingleton<RsaKeyManager>();

        // Repositórios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICodigoValidacaoRepository, CodigoValidacaoRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Serviços de Aplicação
        services.AddScoped<IIdentidadeService, IdentidadeService>();

        // Serviços de Infraestrutura
        services.AddSingleton<ICriptografiaService, CriptografiaService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IEmailService, EmailService>();

        // RabbitMQ Publisher genérico do SharedKernel
        services.AddRabbitMQPublisher(configuration, "agrosolutions.identidade");

        return services;
    }
}




