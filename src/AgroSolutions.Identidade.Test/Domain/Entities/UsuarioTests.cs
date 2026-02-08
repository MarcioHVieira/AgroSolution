using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Domain.Entities;

public class UsuarioTests
{
    [Fact]
    public void Construtor_DeveCriarUsuarioComDadosValidos()
    {
        // Arrange
        var nomeCompleto = "Marcio Henrique";
        var email = "marcio@agrosolutions.com.br";
        var senhaHash = "hashedpassword123";
        var perfil = PerfilAcesso.Usuario;
        var telefone = "11999999999";
        var cpf = "12345678901";

        // Act
        var usuario = new Usuario(nomeCompleto, email, senhaHash, perfil, telefone, cpf);

        // Assert
        usuario.Id.Should().NotBeEmpty();
        usuario.NomeCompleto.Should().Be(nomeCompleto);
        usuario.Email.Should().Be(email.ToLowerInvariant());
        usuario.SenhaHash.Should().Be(senhaHash);
        usuario.Perfil.Should().Be(perfil);
        usuario.Telefone.Should().Be(telefone);
        usuario.Cpf.Should().Be(cpf);
        usuario.Status.Should().Be(StatusUsuario.AguardandoValidacao);
        usuario.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        usuario.Excluido.Should().BeFalse();
    }

    [Fact]
    public void Construtor_DeveConverterEmailParaMinuscula()
    {
        // Arrange & Act
        var usuario = new Usuario("Marcio Henrique", "MARCIO@AGROSOLUTIONS.COM.BR", "hash", PerfilAcesso.Usuario);

        // Assert
        usuario.Email.Should().Be("marcio@agrosolutions.com.br");
    }

    [Theory]
    [InlineData("", "email@agrosolutions.com.br", "hash")]
    [InlineData("   ", "email@agrosolutions.com.br", "hash")]
    [InlineData("Nome", "", "hash")]
    [InlineData("Nome", "   ", "hash")]
    [InlineData("Nome", "email@agrosolutions.com.br", "")]
    [InlineData("Nome", "email@agrosolutions.com.br", "   ")]
    public void Construtor_DeveLancarExcecaoParaDadosInvalidos(string nome, string email, string hash)
    {
        // Act
        var act = () => new Usuario(nome, email, hash, PerfilAcesso.Usuario);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AtualizarSenha_DeveAtualizarSenhaEDataAtualizacao()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "oldHash", PerfilAcesso.Usuario);
        var novaSenhaHash = "newHashedPassword";

        // Act
        usuario.AtualizarSenha(novaSenhaHash);

        // Assert
        usuario.SenhaHash.Should().Be(novaSenhaHash);
        usuario.DataAtualizacao.Should().NotBeNull();
        usuario.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AtualizarSenha_DeveLancarExcecaoParaSenhaInvalida(string? novaSenhaHash)
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "oldHash", PerfilAcesso.Usuario);

        // Act
        var act = () => usuario.AtualizarSenha(novaSenhaHash!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*senha*");
    }

    [Fact]
    public void AtivarConta_DeveAlterarStatusParaAtivo()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);

        // Act
        usuario.AtivarConta();

        // Assert
        usuario.Status.Should().Be(StatusUsuario.Ativo);
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Bloquear_DeveAlterarStatusParaBloqueado()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();

        // Act
        usuario.Bloquear();

        // Assert
        usuario.Status.Should().Be(StatusUsuario.Bloqueado);
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void Desbloquear_DeveAlterarStatusParaAtivo()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);
        usuario.Bloquear();

        // Act
        usuario.Desbloquear();

        // Assert
        usuario.Status.Should().Be(StatusUsuario.Ativo);
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void RegistrarAcesso_DeveAtualizarDataUltimoAcesso()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);

        // Act
        usuario.RegistrarAcesso();

        // Assert
        usuario.DataUltimoAcesso.Should().NotBeNull();
        usuario.DataUltimoAcesso.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AtualizarPerfil_DeveAtualizarDadosDoUsuario()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);
        var novoNome = "Marcio Henrique Santos";
        var novoTelefone = "11888888888";
        var novoCpf = "98765432100";

        // Act
        usuario.AtualizarPerfil(novoNome, novoTelefone, novoCpf);

        // Assert
        usuario.NomeCompleto.Should().Be(novoNome);
        usuario.Telefone.Should().Be(novoTelefone);
        usuario.Cpf.Should().Be(novoCpf);
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void AtualizarPerfil_DeveLancarExcecaoParaNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);

        // Act
        var act = () => usuario.AtualizarPerfil(nomeInvalido!, null, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*nome*");
    }

    [Fact]
    public void MarcarParaExclusao_DeveMarcarUsuarioComoExcluido()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);
        var motivo = "Solicitação do usuário";

        // Act
        usuario.MarcarParaExclusao(motivo);

        // Assert
        usuario.Excluido.Should().BeTrue();
        usuario.DataExclusao.Should().NotBeNull();
        usuario.DataExclusao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        usuario.MotivoExclusao.Should().Be(motivo);
        usuario.Status.Should().Be(StatusUsuario.Inativo);
        usuario.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public void MarcarParaExclusao_DeveUsarMotivoDefaultQuandoNaoFornecido()
    {
        // Arrange
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);

        // Act
        usuario.MarcarParaExclusao();

        // Assert
        usuario.Excluido.Should().BeTrue();
        usuario.MotivoExclusao.Should().Contain("LGPD");
    }
}
