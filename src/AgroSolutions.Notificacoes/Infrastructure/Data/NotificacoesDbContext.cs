using AgroSolutions.Notificacoes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Notificacoes.Infrastructure.Data;

public class NotificacoesDbContext : DbContext
{
    public NotificacoesDbContext(DbContextOptions<NotificacoesDbContext> options) : base(options) { }

    public DbSet<Notificacao> Notificacoes => Set<Notificacao>();
    public DbSet<PropriedadeInfo> PropriedadesInfo => Set<PropriedadeInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("Notificacoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmailDestinatario).IsRequired().HasMaxLength(255);
            entity.Property(e => e.NomeDestinatario).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Assunto).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Mensagem).IsRequired();
            entity.Property(e => e.Tipo).IsRequired().HasConversion<int>();
            entity.Property(e => e.Status).IsRequired().HasConversion<int>();
            entity.Property(e => e.Prioridade).IsRequired().HasConversion<int>();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DestinatarioId);
            entity.HasIndex(e => e.DataCriacao);
        });

        // Configuração da entidade PropriedadeInfo (Read Model)
        modelBuilder.Entity<PropriedadeInfo>(entity =>
        {
            entity.ToTable("PropriedadesInfo");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.EmailProprietario).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NomeProprietario).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DataSincronizacao).IsRequired();
            
            entity.HasIndex(e => e.ProprietarioId);
        });
    }
}
