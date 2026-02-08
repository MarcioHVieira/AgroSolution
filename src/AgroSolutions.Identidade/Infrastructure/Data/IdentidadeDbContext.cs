using AgroSolutions.Identidade.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Identidade.Infrastructure.Data;

public class IdentidadeDbContext : DbContext
{
    public IdentidadeDbContext(DbContextOptions<IdentidadeDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<CodigoValidacao> CodigosValidacao => Set<CodigoValidacao>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditoriaAcesso> AuditoriasAcesso => Set<AuditoriaAcesso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.NomeCompleto)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.SenhaHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Telefone)
                .HasMaxLength(20);

            entity.Property(e => e.Cpf)
                .HasMaxLength(11);

            entity.HasIndex(e => e.Cpf)
                .IsUnique()
                .HasFilter("[Cpf] IS NOT NULL");

            entity.Property(e => e.Perfil)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.HasMany(e => e.CodigosValidacao)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade CodigoValidacao
        modelBuilder.Entity<CodigoValidacao>(entity =>
        {
            entity.ToTable("CodigosValidacao");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Codigo)
                .IsRequired()
                .HasMaxLength(10);

            entity.HasIndex(e => e.Codigo);

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.DataExpiracao)
                .IsRequired();

            entity.Property(e => e.Utilizado)
                .IsRequired();
        });

        // Configuração da entidade RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.Property(e => e.DataCriacao)
                .IsRequired();

            entity.Property(e => e.DataExpiracao)
                .IsRequired();

            entity.Property(e => e.Revogado)
                .IsRequired();

            entity.Property(e => e.MotivoRevogacao)
                .HasMaxLength(500);

            entity.Property(e => e.SubstituidoPor)
                .HasMaxLength(200);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(45);

            entity.HasIndex(e => e.UsuarioId);
        });

        // Configuração da entidade AuditoriaAcesso
        modelBuilder.Entity<AuditoriaAcesso>(entity =>
        {
            entity.ToTable("AuditoriasAcesso");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Acao)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Entidade)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.EnderecoIP)
                .IsRequired()
                .HasMaxLength(45);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(500);

            entity.Property(e => e.DataHora)
                .IsRequired();

            entity.Property(e => e.Sucesso)
                .IsRequired();

            entity.Property(e => e.MensagemErro)
                .HasMaxLength(1000);

            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.DataHora);
            entity.HasIndex(e => new { e.Acao, e.DataHora });

            // Relacionamento com Usuario
            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Auditorias)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

