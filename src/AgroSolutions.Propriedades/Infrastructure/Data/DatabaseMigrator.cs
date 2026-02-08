using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Propriedades.Infrastructure.Data;

/// <summary>
/// Classe para aplicar migrations automaticamente na inicialização
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Aplica todas as migrations pendentes no banco de dados
    /// </summary>
    public static async Task AplicarMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PropriedadesDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<PropriedadesDbContext>>();

        try
        {
            logger.LogInformation("Verificando migrations pendentes...");
            
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Aplicando {Count} migration(s) pendente(s): {Migrations}", 
                    pendingMigrations.Count(), 
                    string.Join(", ", pendingMigrations));
                
                await context.Database.MigrateAsync();
                
                logger.LogInformation("? Migrations aplicadas com sucesso!");
            }
            else
            {
                logger.LogInformation("? Banco de dados já está atualizado. Nenhuma migration pendente.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Erro ao aplicar migrations no banco de dados");
            throw;
        }
    }
}
