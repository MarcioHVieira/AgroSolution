using AgroSolutions.Propriedades.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Propriedades.Infrastructure.Data;

public class PropriedadesDbContext : DbContext
{
    public PropriedadesDbContext(DbContextOptions<PropriedadesDbContext> options) : base(options)
    {
    }

    public DbSet<Propriedade> Propriedades => Set<Propriedade>();
    public DbSet<Talhao> Talhoes => Set<Talhao>();
    public DbSet<Cultura> Culturas => Set<Cultura>();
    public DbSet<UsuarioInfo> UsuariosInfo => Set<UsuarioInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Propriedade
        modelBuilder.Entity<Propriedade>(entity =>
        {
            entity.ToTable("Propriedades");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.AreaTotal).HasPrecision(18, 4);
            entity.Property(e => e.Cep).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Endereco).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Numero).HasMaxLength(20);
            entity.Property(e => e.Complemento).HasMaxLength(100);
            entity.Property(e => e.Bairro).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Cidade).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Estado).IsRequired().HasMaxLength(2);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);

            entity.HasIndex(e => e.ProprietarioId);
            entity.HasIndex(e => new { e.Cidade, e.Estado });

            entity.HasMany(e => e.Talhoes)
                .WithOne(t => t.Propriedade)
                .HasForeignKey(t => t.PropriedadeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade Talhao
        modelBuilder.Entity<Talhao>(entity =>
        {
            entity.ToTable("Talhoes");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Area).HasPrecision(18, 4);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);
            entity.Property(e => e.Poligono).HasColumnType("nvarchar(max)");

            entity.HasIndex(e => e.PropriedadeId);

            entity.HasMany(e => e.Culturas)
                .WithOne(c => c.Talhao)
                .HasForeignKey(c => c.TalhaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade Cultura
        modelBuilder.Entity<Cultura>(entity =>
        {
            entity.ToTable("Culturas");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Variedade).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AreaPlantada).HasPrecision(18, 4);
            entity.Property(e => e.ProducaoEstimada).HasPrecision(18, 2);
            entity.Property(e => e.ProducaoReal).HasPrecision(18, 2);
            entity.Property(e => e.Observacoes).HasMaxLength(1000);

            entity.HasIndex(e => e.TalhaoId);
            entity.HasIndex(e => e.Tipo);
            entity.HasIndex(e => e.Status);
        });

        // Configuração da entidade UsuarioInfo (Read Model)
        modelBuilder.Entity<UsuarioInfo>(entity =>
        {
            entity.ToTable("UsuariosInfo");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.NomeCompleto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DataSincronizacao).IsRequired();

            entity.HasIndex(e => e.Email);
        });
    }
}
