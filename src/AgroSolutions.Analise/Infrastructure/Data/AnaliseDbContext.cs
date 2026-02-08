using AgroSolutions.Analise.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Analise.Infrastructure.Data;

public class AnaliseDbContext : DbContext
{
    public AnaliseDbContext(DbContextOptions<AnaliseDbContext> options) : base(options)
    {
    }

    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<RegraAlerta> RegrasAlertas => Set<RegraAlerta>();
    public DbSet<TalhaoInfo> TalhoesInfo => Set<TalhaoInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Alerta
        modelBuilder.Entity<Alerta>(entity =>
        {
            entity.ToTable("Alertas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Titulo)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Mensagem)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Recomendacao)
                .HasMaxLength(1000);

            entity.Property(e => e.DadosAdicionais)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Severidade)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.DataGeracao)
                .IsRequired();

            entity.HasIndex(e => e.TalhaoId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DataGeracao);
        });

        // Configuração da entidade RegraAlerta
        modelBuilder.Entity<RegraAlerta>(entity =>
        {
            entity.ToTable("RegrasAlertas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descricao)
                .HasMaxLength(500);

            entity.Property(e => e.Condicao)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.TemplateMensagem)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.Recomendacao)
                .HasMaxLength(1000);

            entity.Property(e => e.TipoAlerta)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Severidade)
                .IsRequired()
                .HasConversion<int>();

            entity.HasIndex(e => e.Ativa);
            entity.HasIndex(e => e.TipoAlerta);
        });

        // Configuração da entidade TalhaoInfo (Read Model sincronizado via eventos)
        modelBuilder.Entity<TalhaoInfo>(entity =>
        {
            entity.ToTable("TalhoesInfo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.EmailProprietario)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.NomeProprietario)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.DataSincronizacao)
                .IsRequired();

            entity.HasIndex(e => e.ProprietarioId);
        });
    }
}
