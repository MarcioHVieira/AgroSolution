using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgroSolutions.Identidade;

/// <summary>
/// Factory para criação do DbContext em design-time (migrations, scaffolding, etc)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentidadeDbContext>
{
    public IdentidadeDbContext CreateDbContext(string[] args)
    {
        // Carrega configuração
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionString 'DefaultConnection' não encontrada. " +
                "Configure no appsettings.json ou User Secrets.");
        }

        // Detecta o provider
        var databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";

        var optionsBuilder = new DbContextOptionsBuilder<IdentidadeDbContext>();

        if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseNpgsql(connectionString, 
                b => b.MigrationsAssembly("AgroSolutions.Identidade"));
        }
        else if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(connectionString, 
                b => b.MigrationsAssembly("AgroSolutions.Identidade"));
        }
        else
        {
            throw new InvalidOperationException(
                $"DatabaseProvider '{databaseProvider}' não suportado. " +
                "Use 'SqlServer' ou 'PostgreSQL'");
        }

        // Log da connection string mascarada
        var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"Password=[^;]+", 
            "Password=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        
        Console.WriteLine($"[DesignTime] Usando ConnectionString: {maskedConnectionString}");
        Console.WriteLine($"[DesignTime] Usando Provider: {databaseProvider}");

        return new IdentidadeDbContext(optionsBuilder.Options);
    }
}
