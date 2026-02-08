using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.SharedKernel.Database;

/// <summary>
/// Estratégia de configuração para SQL Server
/// </summary>
public class SqlServerProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "SqlServer";

    public void Configure(DbContextOptionsBuilder options, DatabaseSettings settings)
    {
        options.UseSqlServer(settings.ConnectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: settings.MaxRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(settings.MaxRetryDelaySeconds),
                errorNumbersToAdd: null
            );

            if (settings.CommandTimeoutSeconds > 0)
            {
                sqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
            }
        });
    }

    public string GetHealthCheckName() => "sqlserver";
}

/// <summary>
/// Estratégia de configuração para PostgreSQL
/// </summary>
public class PostgreSqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "PostgreSQL";

    public void Configure(DbContextOptionsBuilder options, DatabaseSettings settings)
    {
        options.UseNpgsql(settings.ConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: settings.MaxRetryCount,
                maxRetryDelay: TimeSpan.FromSeconds(settings.MaxRetryDelaySeconds),
                errorCodesToAdd: null
            );

            if (settings.CommandTimeoutSeconds > 0)
            {
                npgsqlOptions.CommandTimeout(settings.CommandTimeoutSeconds);
            }
        });
    }

    public string GetHealthCheckName() => "postgresql";
}
