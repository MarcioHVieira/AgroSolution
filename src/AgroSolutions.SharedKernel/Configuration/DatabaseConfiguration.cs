using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Helpers unificados para configuração de banco de dados em todos os microserviços
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Configura o DbContext com SQL Server ou PostgreSQL baseado na configuração
    /// </summary>
    /// <param name="optionsBuilder">O DbContextOptionsBuilder a ser configurado</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <param name="connectionStringName">Nome da connection string (padrão: "DefaultConnection")</param>
    /// <param name="databaseProviderKey">Chave da configuração do provider (padrão: "DatabaseProvider")</param>
    public static void ConfigureDatabase(
        DbContextOptionsBuilder optionsBuilder,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        string databaseProviderKey = "DatabaseProvider")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionString '{connectionStringName}' não configurada. " +
                $"Configure via appsettings.json ou variável de ambiente 'ConnectionStrings__{connectionStringName}'");
        }

        // Detecta o provider baseado na configuração (padrão: SQL Server)
        var databaseProvider = configuration[databaseProviderKey] ?? "SqlServer";

        if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString);
        }
        else if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        else
        {
            throw new InvalidOperationException(
                $"DatabaseProvider '{databaseProvider}' não suportado. " +
                $"Use 'SqlServer' ou 'PostgreSQL'");
        }
    }

    /// <summary>
    /// Mascara a senha na connection string para logs seguros
    /// </summary>
    public static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "null";

        return Regex.Replace(
            connectionString, 
            @"Password=[^;]+", 
            "Password=***", 
            RegexOptions.IgnoreCase
        );
    }

    /// <summary>
    /// Obtém a connection string e valida se está configurada
    /// </summary>
    public static string GetRequiredConnectionString(
        IConfiguration configuration, 
        string connectionStringName = "DefaultConnection")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"ConnectionString '{connectionStringName}' não configurada. " +
                $"Configure via appsettings.json ou variável de ambiente 'ConnectionStrings__{connectionStringName}'");
        }

        return connectionString;
    }
}
