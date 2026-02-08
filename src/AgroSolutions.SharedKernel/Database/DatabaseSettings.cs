namespace AgroSolutions.SharedKernel.Database;

/// <summary>
/// Configurações de banco de dados
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Provider do banco de dados (SqlServer, PostgreSQL)
    /// </summary>
    public string Provider { get; set; } = "SqlServer";

    /// <summary>
    /// String de conexão
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Número máximo de tentativas de retry em caso de falha
    /// </summary>
    public int MaxRetryCount { get; set; } = 5;

    /// <summary>
    /// Delay máximo entre tentativas (em segundos)
    /// </summary>
    public int MaxRetryDelaySeconds { get; set; } = 30;

    /// <summary>
    /// Timeout de comando (em segundos)
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Timeout para Health Check (em segundos)
    /// </summary>
    public int HealthCheckTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Se deve aplicar migrations automaticamente na inicialização
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; } = true;
}
