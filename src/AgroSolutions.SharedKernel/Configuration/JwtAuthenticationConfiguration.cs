using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configuração unificada de autenticação JWT para todos os microserviços
/// Valida tokens emitidos pelo microserviço de Identidade usando chave pública RSA
/// </summary>
public static class JwtAuthenticationConfiguration
{
    /// <summary>
    /// Adiciona autenticação JWT usando JWKS
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var identidadeUrl = configuration["Identidade:Url"] 
            ?? throw new InvalidOperationException("URL do microserviço de Identidade não configurada. Configure 'Identidade:Url' no appsettings.json");

        var issuer = configuration["Jwt:Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer não configurado. Configure 'Jwt:Issuer' no appsettings.json");
        
        var audience = configuration["Jwt:Audience"] 
            ?? throw new InvalidOperationException("JWT Audience não configurado. Configure 'Jwt:Audience' no appsettings.json");

        // Em desenvolvimento, não exige HTTPS para facilitar testes locais
        var requireHttpsMetadata = configuration.GetValue<bool>("Jwt:RequireHttpsMetadata", !environment.IsDevelopment());
        
        // Clock skew para tolerar diferenças de relógio entre servidores
        var clockSkewSeconds = configuration.GetValue<int>("Jwt:ClockSkewSeconds", 0);
        
        // Logs detalhados apenas em desenvolvimento
        var enableDetailedLogs = configuration.GetValue<bool>("Jwt:EnableDetailedLogs", environment.IsDevelopment());

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Configuração usando JWKS (automaticamente baixa e valida usando a chave pública RSA)
            options.Authority = identidadeUrl;
            options.RequireHttpsMetadata = requireHttpsMetadata;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ClockSkew = TimeSpan.FromSeconds(clockSkewSeconds)
            };

            // Eventos para logging e debug
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    
                    logger.LogWarning(
                        "? Falha na autenticação JWT: {Message}", 
                        context.Exception.Message
                    );
                    
                    if (enableDetailedLogs)
                    {
                        logger.LogDebug(
                            "Detalhes do erro JWT: {Exception}", 
                            context.Exception.ToString()
                        );
                    }
                    
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    if (enableDetailedLogs)
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        
                        var userId = context.Principal?.FindFirst("sub")?.Value ?? "unknown";
                        var email = context.Principal?.FindFirst("email")?.Value ?? "unknown";
                        
                        logger.LogDebug(
                            "? Token validado | UserId: {UserId} | Email: {Email}", 
                            userId, 
                            email
                        );
                    }
                    
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    if (enableDetailedLogs)
                    {
                        var logger = context.HttpContext.RequestServices
                            .GetRequiredService<ILogger<JwtBearerEvents>>();
                        
                        logger.LogWarning(
                            "?? JWT Challenge | Path: {Path} | Error: {Error}", 
                            context.Request.Path,
                            context.Error ?? "none"
                        );
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
