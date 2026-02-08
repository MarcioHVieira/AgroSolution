using AgroSolutions.Notificacoes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgroSolutions.Notificacoes;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<NotificacoesDbContext>
{
    public NotificacoesDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<NotificacoesDbContext>();
        
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' não encontrada. " +
                "Verifique appsettings.json ou user secrets.");
        }

        optionsBuilder.UseSqlServer(connectionString);

        return new NotificacoesDbContext(optionsBuilder.Options);
    }
}
