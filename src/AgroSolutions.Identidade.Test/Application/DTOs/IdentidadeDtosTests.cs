using AgroSolutions.Identidade.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Application.DTOs;

public class IdentidadeDtosTests
{
    [Fact]
    public void RegistrarUsuarioDto_DeveCriarComDadosCompletos()
    {
        // Act
        var dto = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", "11999999999", "12345678909");

        // Assert
        dto.NomeCompleto.Should().Be("Marcio Henrique");
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Senha.Should().Be("Senha@123");
        dto.Telefone.Should().Be("11999999999");
        dto.Cpf.Should().Be("12345678909");
    }

    [Fact]
    public void RegistrarUsuarioDto_DeveCriarComDadosOpcionaisNulos()
    {
        // Act
        var dto = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, null);

        // Assert
        dto.NomeCompleto.Should().Be("Marcio Henrique");
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Senha.Should().Be("Senha@123");
        dto.Telefone.Should().BeNull();
        dto.Cpf.Should().BeNull();
    }

    [Fact]
    public void ValidarCodigoDto_DeveCriar()
    {
        // Act
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");

        // Assert
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Codigo.Should().Be("123456");
    }

    [Fact]
    public void LoginDto_DeveCriar()
    {
        // Act
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");

        // Assert
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Senha.Should().Be("Senha@123");
    }

    [Fact]
    public void EsqueciSenhaDto_DeveCriar()
    {
        // Act
        var dto = new EsqueciSenhaDto("marcio@agrosolutions.com.br");

        // Assert
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
    }

    [Fact]
    public void RedefinirSenhaDto_DeveCriar()
    {
        // Act
        var dto = new RedefinirSenhaDto("marcio@agrosolutions.com.br", "123456", "NovaSenha@123");

        // Assert
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Codigo.Should().Be("123456");
        dto.NovaSenha.Should().Be("NovaSenha@123");
    }

    [Fact]
    public void AlterarSenhaDto_DeveCriar()
    {
        // Act
        var dto = new AlterarSenhaDto("SenhaAtual@123", "NovaSenha@456");

        // Assert
        dto.SenhaAtual.Should().Be("SenhaAtual@123");
        dto.NovaSenha.Should().Be("NovaSenha@456");
    }

    [Fact]
    public void TokenResponseDto_DeveCriar()
    {
        // Arrange
        var usuarioDto = new UsuarioDto(
            Guid.NewGuid(),
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "11999999999",
            "12345678909",
            "Usuario",
            "Ativo",
            DateTime.UtcNow
        );

        // Act
        var dto = new TokenResponseDto("access-token", "refresh-token", "Bearer", 3600, usuarioDto);

        // Assert
        dto.AccessToken.Should().Be("access-token");
        dto.RefreshToken.Should().Be("refresh-token");
        dto.TokenType.Should().Be("Bearer");
        dto.ExpiresIn.Should().Be(3600);
        dto.Usuario.Should().Be(usuarioDto);
    }

    [Fact]
    public void RefreshTokenDto_DeveCriar()
    {
        // Act
        var dto = new RefreshTokenDto("refresh-token");

        // Assert
        dto.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public void UsuarioDto_DeveCriar()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dataCriacao = DateTime.UtcNow;

        // Act
        var dto = new UsuarioDto(id, "Marcio Henrique", "marcio@agrosolutions.com.br", "11999999999", "12345678909", "Usuario", "Ativo", dataCriacao);

        // Assert
        dto.Id.Should().Be(id);
        dto.NomeCompleto.Should().Be("Marcio Henrique");
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Telefone.Should().Be("11999999999");
        dto.Cpf.Should().Be("12345678909");
        dto.Perfil.Should().Be("Usuario");
        dto.Status.Should().Be("Ativo");
        dto.DataCriacao.Should().Be(dataCriacao);
    }

    [Fact]
    public void UsuarioDto_DeveCriarComDadosOpcionaisNulos()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dataCriacao = DateTime.UtcNow;

        // Act
        var dto = new UsuarioDto(id, "Marcio Henrique", "marcio@agrosolutions.com.br", null, null, "Usuario", "Ativo", dataCriacao);

        // Assert
        dto.Id.Should().Be(id);
        dto.NomeCompleto.Should().Be("Marcio Henrique");
        dto.Email.Should().Be("marcio@agrosolutions.com.br");
        dto.Telefone.Should().BeNull();
        dto.Cpf.Should().BeNull();
        dto.Perfil.Should().Be("Usuario");
        dto.Status.Should().Be("Ativo");
        dto.DataCriacao.Should().Be(dataCriacao);
    }

    [Fact]
    public void RegistroResponseDto_DeveCriar()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();

        // Act
        var dto = new RegistroResponseDto(usuarioId);

        // Assert
        dto.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    public void ConfirmarSenhaDto_DeveCriar()
    {
        // Act
        var dto = new ConfirmarSenhaDto("Senha@123");

        // Assert
        dto.Senha.Should().Be("Senha@123");
    }

    [Fact]
    public void RegistrarUsuarioDto_RecordsDevemSerImutaveis()
    {
        // Arrange
        var dto1 = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, null);
        var dto2 = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, null);
        var dto3 = new RegistrarUsuarioDto("Lucas Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, null);

        // Assert
        dto1.Should().Be(dto2); // Records com mesmos valores são iguais
        dto1.Should().NotBe(dto3); // Records com valores diferentes são diferentes
    }

    [Fact]
    public void TokenResponseDto_RecordsDevemSerImutaveis()
    {
        // Arrange
        var usuarioDto = new UsuarioDto(
            Guid.NewGuid(),
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            null,
            null,
            "Usuario",
            "Ativo",
            DateTime.UtcNow
        );

        var dto1 = new TokenResponseDto("token1", "refresh1", "Bearer", 3600, usuarioDto);
        var dto2 = new TokenResponseDto("token1", "refresh1", "Bearer", 3600, usuarioDto);
        var dto3 = new TokenResponseDto("token2", "refresh1", "Bearer", 3600, usuarioDto);

        // Assert
        dto1.Should().Be(dto2);
        dto1.Should().NotBe(dto3);
    }
}
