using AgroSolutions.Identidade.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Domain.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void Construtor_DeveCriarRefreshTokenComDadosValidos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var token = "refreshtoken123";
        var dataExpiracao = DateTime.UtcNow.AddDays(7);
        var ipAddress = "192.168.0.1";

        // Act
        var refreshToken = new RefreshToken(usuarioId, token, dataExpiracao, ipAddress);

        // Assert
        refreshToken.Id.Should().NotBeEmpty();
        refreshToken.UsuarioId.Should().Be(usuarioId);
        refreshToken.Token.Should().Be(token);
        refreshToken.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        refreshToken.DataExpiracao.Should().Be(dataExpiracao);
        refreshToken.Revogado.Should().BeFalse();
        refreshToken.IpAddress.Should().Be(ipAddress);
    }

    [Fact]
    public void Construtor_DeveLancarExcecaoParaTokenNulo()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var dataExpiracao = DateTime.UtcNow.AddDays(7);

        // Act
        var act = () => new RefreshToken(usuarioId, null!, dataExpiracao);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("token");
    }

    [Fact]
    public void EstaValido_DeveRetornarTrueParaTokenValidoENaoRevogado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddDays(7)
        );

        // Act
        var resultado = refreshToken.EstaValido();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EstaValido_DeveRetornarFalseParaTokenRevogado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddDays(7)
        );
        refreshToken.Revogar("Teste");

        // Act
        var resultado = refreshToken.EstaValido();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EstaValido_DeveRetornarFalseParaTokenExpirado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddSeconds(-1) // Expirado
        );

        // Act
        var resultado = refreshToken.EstaValido();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EstaExpirado_DeveRetornarTrueParaTokenExpirado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddSeconds(-1)
        );

        // Act
        var resultado = refreshToken.EstaExpirado();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EstaExpirado_DeveRetornarFalseParaTokenNaoExpirado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddDays(7)
        );

        // Act
        var resultado = refreshToken.EstaExpirado();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void Revogar_DeveMarcarTokenComoRevogado()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddDays(7)
        );
        var motivo = "Novo login realizado";
        var substituidoPor = "newtoken456";

        // Act
        refreshToken.Revogar(motivo, substituidoPor);

        // Assert
        refreshToken.Revogado.Should().BeTrue();
        refreshToken.DataRevogacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        refreshToken.MotivoRevogacao.Should().Be(motivo);
        refreshToken.SubstituidoPor.Should().Be(substituidoPor);
    }

    [Fact]
    public void Revogar_DeveFuncionarSemTokenSubstituto()
    {
        // Arrange
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            "token123",
            DateTime.UtcNow.AddDays(7)
        );
        var motivo = "Revogação manual";

        // Act
        refreshToken.Revogar(motivo);

        // Assert
        refreshToken.Revogado.Should().BeTrue();
        refreshToken.MotivoRevogacao.Should().Be(motivo);
        refreshToken.SubstituidoPor.Should().BeNull();
    }
}
