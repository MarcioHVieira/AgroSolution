using AgroSolutions.SharedKernel.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configuração centralizada e padronizada de banco de dados para todos os microserviços
/// Suporta múltiplos providers através do Strategy Pattern
/// </summary>
public static class StandardDatabaseConfiguration
{
    private static readonly Dictionary<string, IDatabaseProviderStrategy> _strategies = new()
    {
        { "SqlServer", new SqlServerProviderStrategy() },
        { "PostgreSQL", new PostgreSqlProviderStrategy() }
    };

    /// <summary>
    /// Adiciona configuração de banco de dados com detecção automática de provider
    /// </summary>
    /// <typeparam name="TContext">Tipo do DbContext</typeparam>
    public static IServiceCollection AddStandardDatabaseConfiguration<TContext>(
        this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionString 'DefaultConnection' não configurada. " +
                "Configure via appsettings.json ou variável de ambiente 'ConnectionStrings__DefaultConnection'");
        }

        // Lê configurações
        var settings = new DatabaseSettings
        {
            ConnectionString = connectionString,
            Provider = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer",
            MaxRetryCount = configuration.GetValue<int>("Database:MaxRetryCount", 5),
            MaxRetryDelaySeconds = configuration.GetValue<int>("Database:MaxRetryDelaySeconds", 30),
            CommandTimeoutSeconds = configuration.GetValue<int>("Database:CommandTimeoutSeconds", 30),
            HealthCheckTimeoutSeconds = configuration.GetValue<int>("Database:HealthCheckTimeoutSeconds", 10),
            ApplyMigrationsOnStartup = configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup", true)
        };

        // Obtém estratégia do provider
        if (!_strategies.TryGetValue(settings.Provider, out var strategy))
        {
            throw new InvalidOperationException(
                $"DatabaseProvider '{settings.Provider}' não suportado. " +
                $"Providers suportados: {string.Join(", ", _strategies.Keys)}");
        }

        // Configura DbContext usando a estratégia
        services.AddDbContext<TContext>(options =>
        {
            strategy.Configure(options, settings);
        });

        // Adiciona Health Check
        var healthCheckBuilder = services.AddHealthChecks();
        
        if (settings.Provider == "SqlServer")
        {
            healthCheckBuilder.AddSqlServer(
                connectionString,
                name: strategy.GetHealthCheckName(),
                timeout: TimeSpan.FromSeconds(settings.HealthCheckTimeoutSeconds)
            );
        }
        else if (settings.Provider == "PostgreSQL")
        {
            healthCheckBuilder.AddNpgSql(
                connectionString,
                name: strategy.GetHealthCheckName(),
                timeout: TimeSpan.FromSeconds(settings.HealthCheckTimeoutSeconds)
            );
        }

        return services;
    }

    /// <summary>
    /// Aplica migrations do banco de dados na inicialização da aplicação
    /// </summary>
    public static WebApplication ApplyStandardDatabaseMigrations<TContext>(
        this WebApplication app) where TContext : DbContext
    {
        var configuration = app.Configuration;
        var applyMigrations = configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup", true);

        if (!applyMigrations)
        {
            app.Logger.LogInformation("?? Migrations automáticas desabilitadas (Database:ApplyMigrationsOnStartup = false)");
            return app;
        }

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        
        try
        {
            var dbName = dbContext.Database.GetDbConnection().Database;
            app.Logger.LogInformation("?? Aplicando migrações do banco de dados: {DatabaseName}...", dbName);
            
            dbContext.Database.Migrate();
            
            app.Logger.LogInformation("? Migrações aplicadas com sucesso!");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "? Erro ao aplicar migrações do banco de dados");
            
            // Em ambiente de produção, pode ser desejável falhar o startup
            if (app.Environment.EnvironmentName == "Production")
            {
                throw;
            }
        }

        return app;
    }
}
