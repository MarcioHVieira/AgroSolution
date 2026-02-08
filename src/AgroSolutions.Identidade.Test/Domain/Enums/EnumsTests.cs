using AgroSolutions.Identidade.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Domain.Enums;

public class PerfilAcessoTests
{
    [Fact]
    public void PerfilAcesso_DeveConterUsuario()
    {
        // Assert
        PerfilAcesso.Usuario.Should().Be((PerfilAcesso)1);
        PerfilAcesso.Usuario.ToString().Should().Be("Usuario");
    }

    [Fact]
    public void PerfilAcesso_DeveConterAdministrador()
    {
        // Assert
        PerfilAcesso.Administrador.Should().Be((PerfilAcesso)2);
        PerfilAcesso.Administrador.ToString().Should().Be("Administrador");
    }

    [Fact]
    public void PerfilAcesso_DeveSerConversiveisParaInt()
    {
        // Arrange
        var usuario = PerfilAcesso.Usuario;
        var administrador = PerfilAcesso.Administrador;

        // Act
        var usuarioInt = (int)usuario;
        var administradorInt = (int)administrador;

        // Assert
        usuarioInt.Should().Be(1);
        administradorInt.Should().Be(2);
    }

    [Fact]
    public void PerfilAcesso_DeveSerConversiveisDeInt()
    {
        // Act
        var usuario = (PerfilAcesso)1;
        var administrador = (PerfilAcesso)2;

        // Assert
        usuario.Should().Be(PerfilAcesso.Usuario);
        administrador.Should().Be(PerfilAcesso.Administrador);
    }
}

public class StatusUsuarioTests
{
    [Fact]
    public void StatusUsuario_DeveConterAguardandoValidacao()
    {
        // Assert
        StatusUsuario.AguardandoValidacao.Should().Be((StatusUsuario)1);
        StatusUsuario.AguardandoValidacao.ToString().Should().Be("AguardandoValidacao");
    }

    [Fact]
    public void StatusUsuario_DeveConterAtivo()
    {
        // Assert
        StatusUsuario.Ativo.Should().Be((StatusUsuario)2);
        StatusUsuario.Ativo.ToString().Should().Be("Ativo");
    }

    [Fact]
    public void StatusUsuario_DeveConterBloqueado()
    {
        // Assert
        StatusUsuario.Bloqueado.Should().Be((StatusUsuario)3);
        StatusUsuario.Bloqueado.ToString().Should().Be("Bloqueado");
    }

    [Fact]
    public void StatusUsuario_DeveConterInativo()
    {
        // Assert
        StatusUsuario.Inativo.Should().Be((StatusUsuario)4);
        StatusUsuario.Inativo.ToString().Should().Be("Inativo");
    }

    [Fact]
    public void StatusUsuario_DeveSerConversiveisParaInt()
    {
        // Arrange
        var aguardandoValidacao = StatusUsuario.AguardandoValidacao;
        var ativo = StatusUsuario.Ativo;
        var bloqueado = StatusUsuario.Bloqueado;
        var inativo = StatusUsuario.Inativo;

        // Act
        var aguardandoValidacaoInt = (int)aguardandoValidacao;
        var ativoInt = (int)ativo;
        var bloqueadoInt = (int)bloqueado;
        var inativoInt = (int)inativo;

        // Assert
        aguardandoValidacaoInt.Should().Be(1);
        ativoInt.Should().Be(2);
        bloqueadoInt.Should().Be(3);
        inativoInt.Should().Be(4);
    }

    [Fact]
    public void StatusUsuario_DeveSerConversiveisDeInt()
    {
        // Act
        var aguardandoValidacao = (StatusUsuario)1;
        var ativo = (StatusUsuario)2;
        var bloqueado = (StatusUsuario)3;
        var inativo = (StatusUsuario)4;

        // Assert
        aguardandoValidacao.Should().Be(StatusUsuario.AguardandoValidacao);
        ativo.Should().Be(StatusUsuario.Ativo);
        bloqueado.Should().Be(StatusUsuario.Bloqueado);
        inativo.Should().Be(StatusUsuario.Inativo);
    }
}
