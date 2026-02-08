using AgroSolutions.SharedKernel.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Extension methods para configuração padronizada de microserviços
/// </summary>
public static class MicroserviceExtensions
{
    /// <summary>
    /// Registra o middleware de tratamento de exceções
    /// </summary>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
    
    /// <summary>
    /// Adiciona configuração padrão de serviços para microserviços com banco de dados
    /// </summary>
    /// <typeparam name="TDbContext">Tipo do DbContext do microserviço</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="environment">Host environment</param>
    /// <param name="requiresAuth">Se o microserviço requer autenticação JWT (padrão: true)</param>
    /// <returns>Service collection para encadeamento</returns>
    public static IServiceCollection AddStandardMicroserviceServices<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        bool requiresAuth = true)
        where TDbContext : DbContext
    {
        services.AddStandardDatabaseConfiguration<TDbContext>(configuration);
        
        if (requiresAuth)
        {
            services.AddJwtAuthentication(configuration, environment);
        }
        
        return services;
    }
    
    /// <summary>
    /// Adiciona configuração padrão de serviços para microserviços SEM banco de dados
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="environment">Host environment</param>
    /// <param name="requiresAuth">Se o microserviço requer autenticação JWT (padrão: true)</param>
    /// <returns>Service collection para encadeamento</returns>
    public static IServiceCollection AddStandardMicroserviceServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        bool requiresAuth = true)
    {
        if (requiresAuth)
        {
            services.AddJwtAuthentication(configuration, environment);
        }
        
        return services;
    }

    /// <summary>
    /// Configura o pipeline HTTP padrão para microserviços
    /// </summary>
    /// <param name="app">WebApplication</param>
    /// <param name="requiresAuth">Se o microserviço requer autenticação JWT (padrão: true)</param>
    /// <param name="logStartupInfo">Se deve logar informações de inicialização (padrão: true)</param>
    /// <param name="serviceName">Nome do serviço para logging (opcional)</param>
    /// <returns>WebApplication para encadeamento</returns>
    public static WebApplication UseStandardMicroservicePipeline(
        this WebApplication app,
        bool requiresAuth = true,
        bool logStartupInfo = true,
        string? serviceName = null)
    {
        app.UseUtf8Encoding();
        app.UseExceptionHandling();
        
        if (requiresAuth)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
        else
        {
            app.UseAuthorization();
        }
        
        app.MapControllers();
        
        if (logStartupInfo)
        {
            LogMicroserviceStartup(app, serviceName);
        }
        
        return app;
    }
    
    /// <summary>
    /// Configura o pipeline HTTP padrão para microserviços COM banco de dados
    /// </summary>
    /// <typeparam name="TDbContext">Tipo do DbContext do microserviço</typeparam>
    /// <param name="app">WebApplication</param>
    /// <param name="requiresAuth">Se o microserviço requer autenticação JWT (padrão: true)</param>
    /// <param name="logStartupInfo">Se deve logar informações de inicialização (padrão: true)</param>
    /// <param name="serviceName">Nome do serviço para logging (opcional)</param>
    /// <returns>WebApplication para encadeamento</returns>
    public static WebApplication UseStandardMicroservicePipeline<TDbContext>(
        this WebApplication app,
        bool requiresAuth = true,
        bool logStartupInfo = true,
        string? serviceName = null)
        where TDbContext : DbContext
    {
        app.UseUtf8Encoding();
        app.UseExceptionHandling();
        
        if (requiresAuth)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
        else
        {
            app.UseAuthorization();
        }
        
        app.MapControllers();
        
        app.ApplyStandardDatabaseMigrations<TDbContext>();
        
        if (logStartupInfo)
        {
            LogMicroserviceStartup(app, serviceName);
        }
        
        return app;
    }
    
    /// <summary>
    /// Loga informações de inicialização do microserviço
    /// </summary>
    private static void LogMicroserviceStartup(WebApplication app, string? serviceName)
    {
        var service = serviceName ?? app.Environment.ApplicationName;
        
        app.Logger.LogInformation("{ServiceName} iniciado com sucesso", service);
        app.Logger.LogInformation("Ambiente: {Environment}", app.Environment.EnvironmentName);
        app.Logger.LogInformation("URLs: {Urls}", string.Join(", ", app.Urls));
    }
}