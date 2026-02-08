using AgroSolutions.Identidade.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Domain.Entities;

public class CodigoValidacaoTests
{
    [Fact]
    public void Construtor_DeveCriarCodigoValidacaoComDadosValidos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var codigo = "123456";
        var minutosValidade = 30;

        // Act
        var codigoValidacao = new CodigoValidacao(usuarioId, codigo, minutosValidade);

        // Assert
        codigoValidacao.Id.Should().NotBeEmpty();
        codigoValidacao.UsuarioId.Should().Be(usuarioId);
        codigoValidacao.Codigo.Should().Be(codigo);
        codigoValidacao.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        codigoValidacao.DataExpiracao.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(minutosValidade), TimeSpan.FromSeconds(5));
        codigoValidacao.Utilizado.Should().BeFalse();
        codigoValidacao.DataUtilizacao.Should().BeNull();
    }

    [Fact]
    public void Construtor_DeveUsarValorPadraoParaMinutosValidade()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var codigo = "123456";

        // Act
        var codigoValidacao = new CodigoValidacao(usuarioId, codigo);

        // Assert
        codigoValidacao.DataExpiracao.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EstaValido_DeveRetornarTrueParaCodigoNaoUtilizadoENaoExpirado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", 30);

        // Act
        var resultado = codigoValidacao.EstaValido();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EstaValido_DeveRetornarFalseParaCodigoUtilizado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", 30);
        codigoValidacao.MarcarComoUtilizado();

        // Act
        var resultado = codigoValidacao.EstaValido();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EstaValido_DeveRetornarFalseParaCodigoExpirado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", -1); // Expirado

        // Act
        var resultado = codigoValidacao.EstaValido();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void MarcarComoUtilizado_DeveMarcarCodigoComoUtilizado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", 30);

        // Act
        codigoValidacao.MarcarComoUtilizado();

        // Assert
        codigoValidacao.Utilizado.Should().BeTrue();
        codigoValidacao.DataUtilizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void EstaExpirado_DeveRetornarTrueParaCodigoExpirado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", -1);

        // Act
        var resultado = codigoValidacao.EstaExpirado();

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EstaExpirado_DeveRetornarFalseParaCodigoNaoExpirado()
    {
        // Arrange
        var codigoValidacao = new CodigoValidacao(Guid.NewGuid(), "123456", 30);

        // Act
        var resultado = codigoValidacao.EstaExpirado();

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EstaValido_DeveRetornarTrueParaCodigoRecemCriado()
    {
        // Arrange - criar um código com validade positiva
        var usuarioId = Guid.NewGuid();
        var codigo = "123456";
        var codigoValidacao = new CodigoValidacao(usuarioId, codigo, 30);

        // Act
        var resultado = codigoValidacao.EstaValido();

        // Assert
        // Deve ser válido quando DateTime.UtcNow <= DataExpiracao
        resultado.Should().BeTrue();
    }
}
