using AgroSolutions.IngestaoDados.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.IngestaoDados.Infrastructure.Data;

public class IngestaoDbContext : DbContext
{
    public IngestaoDbContext(DbContextOptions<IngestaoDbContext> options) : base(options)
    {
    }

    public DbSet<Sensor> Sensores { get; set; }
    public DbSet<LeituraSensor> Leituras { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Sensor
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("Sensores");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PropriedadeId).IsRequired();
            entity.Property(e => e.TalhaoId).IsRequired(false);
            
            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.HasIndex(e => e.DeviceId)
                .IsUnique()
                .HasDatabaseName("IX_Sensores_DeviceId");

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Fabricante)
                .HasMaxLength(100);

            entity.Property(e => e.Modelo)
                .HasMaxLength(100);

            entity.Property(e => e.Latitude)
                .HasPrecision(10, 7);

            entity.Property(e => e.Longitude)
                .HasPrecision(10, 7);

            entity.Property(e => e.Altitude)
                .HasPrecision(8, 2);

            entity.Property(e => e.IntervaloLeituraMinutos)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.UltimaLeitura)
                .IsRequired(false);

            entity.Property(e => e.UltimaCalibracao)
                .IsRequired(false);

            entity.Property(e => e.Observacoes)
                .HasMaxLength(1000);

            entity.Property(e => e.DataCadastro)
                .IsRequired();

            entity.Property(e => e.DataAtualizacao)
                .IsRequired(false);

            // Índices
            entity.HasIndex(e => e.PropriedadeId)
                .HasDatabaseName("IX_Sensores_PropriedadeId");

            entity.HasIndex(e => e.TalhaoId)
                .HasDatabaseName("IX_Sensores_TalhaoId");

            entity.HasIndex(e => e.Tipo)
                .HasDatabaseName("IX_Sensores_Tipo");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Sensores_Status");

            // Relacionamentos
            entity.HasMany(e => e.Leituras)
                .WithOne(e => e.Sensor)
                .HasForeignKey(e => e.SensorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade LeituraSensor
        modelBuilder.Entity<LeituraSensor>(entity =>
        {
            entity.ToTable("Leituras");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SensorId).IsRequired();

            entity.Property(e => e.Valor)
                .IsRequired()
                .HasPrecision(18, 4);

            entity.Property(e => e.Unidade)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.TimestampLeitura)
                .IsRequired();

            entity.Property(e => e.TimestampRecebimento)
                .IsRequired();

            entity.Property(e => e.Qualidade)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.NivelBateria)
                .IsRequired(false);

            entity.Property(e => e.IntensidadeSinal)
                .IsRequired(false);

            entity.Property(e => e.DadosAdicionais)
                .HasMaxLength(4000);

            entity.Property(e => e.Observacoes)
                .HasMaxLength(500);

            // Índices
            entity.HasIndex(e => e.SensorId)
                .HasDatabaseName("IX_Leituras_SensorId");

            entity.HasIndex(e => e.TimestampLeitura)
                .HasDatabaseName("IX_Leituras_TimestampLeitura");

            entity.HasIndex(e => new { e.SensorId, e.TimestampLeitura })
                .HasDatabaseName("IX_Leituras_SensorId_TimestampLeitura");

            entity.HasIndex(e => e.Qualidade)
                .HasDatabaseName("IX_Leituras_Qualidade");
        });
    }
}

