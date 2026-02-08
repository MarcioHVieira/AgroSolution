using AgroSolutions.ProcessamentoDados.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.ProcessamentoDados.Infrastructure.Data;

public class ProcessamentoDbContext : DbContext
{
    public ProcessamentoDbContext(DbContextOptions<ProcessamentoDbContext> options)
        : base(options)
    {
    }

    public DbSet<LeituraProcessada> LeiturasProcessadas { get; set; }
    public DbSet<AgregacaoDados> AgregacoesDados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração LeituraProcessada
        modelBuilder.Entity<LeituraProcessada>(entity =>
        {
            entity.ToTable("LeiturasProcessadas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Unidade)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Valor)
                .HasPrecision(18, 4);

            entity.Property(e => e.Status)
                .HasConversion<int>();

            entity.Property(e => e.Qualidade)
                .HasConversion<int>();

            entity.Property(e => e.TipoSensor)
                .HasConversion<int>();

            entity.Property(e => e.MensagemErro)
                .HasMaxLength(1000);

            // Índices para performance
            entity.HasIndex(e => e.LeituraOrigemId);
            entity.HasIndex(e => e.SensorId);
            entity.HasIndex(e => e.PropriedadeId);
            entity.HasIndex(e => e.TalhaoId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.TimestampLeitura);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.SensorId, e.TimestampLeitura });
        });

        // Configuração AgregacaoDados
        modelBuilder.Entity<AgregacaoDados>(entity =>
        {
            entity.ToTable("AgregacoesDados");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Unidade)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ValorMinimo)
                .HasPrecision(18, 4);

            entity.Property(e => e.ValorMaximo)
                .HasPrecision(18, 4);

            entity.Property(e => e.ValorMedio)
                .HasPrecision(18, 4);

            entity.Property(e => e.DesvioPadrao)
                .HasPrecision(18, 4);

            entity.Property(e => e.TipoSensor)
                .HasConversion<int>();

            entity.Property(e => e.TipoAgregacao)
                .HasConversion<int>();

            // Índices para performance
            entity.HasIndex(e => e.SensorId);
            entity.HasIndex(e => e.PropriedadeId);
            entity.HasIndex(e => e.TalhaoId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => e.TipoAgregacao);
            entity.HasIndex(e => e.PeriodoInicio);
            entity.HasIndex(e => new { e.SensorId, e.TipoAgregacao, e.PeriodoInicio })
                .IsUnique();
        });
    }
}
