using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.SharedKernel.Database;

/// <summary>
/// Interface para estratégia de configuração de database provider
/// Implementa o Strategy Pattern para suportar diferentes providers
/// </summary>
public interface IDatabaseProviderStrategy
{
    /// <summary>
    /// Nome do provider (SqlServer, PostgreSQL, etc)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Configura o DbContext com o provider específico
    /// </summary>
    void Configure(DbContextOptionsBuilder options, DatabaseSettings settings);

    /// <summary>
    /// Retorna o nome do Health Check do provider
    /// </summary>
    string GetHealthCheckName();
}
