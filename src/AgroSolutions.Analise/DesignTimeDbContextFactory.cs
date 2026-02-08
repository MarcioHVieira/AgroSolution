using AgroSolutions.Analise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgroSolutions.Analise;

/// <summary>
/// Factory para criação do DbContext em design-time (migrations)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AnaliseDbContext>
{
    public AnaliseDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AnaliseDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não encontrada. " +
                "Verifique appsettings.json ou user secrets.");
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new AnaliseDbContext(optionsBuilder.Options);
    }
}

