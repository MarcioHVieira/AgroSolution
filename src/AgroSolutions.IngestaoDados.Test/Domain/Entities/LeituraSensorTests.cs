using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.IngestaoDados.Test.Domain.Entities;

public class LeituraSensorTests
{
    [Fact]
    public void Construtor_DeveCriarLeituraComDadosValidos()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var valor = 25.5m;
        var unidade = "°C";
        var timestampLeitura = DateTime.UtcNow.AddMinutes(-5);
        var qualidade = QualidadeLeitura.Normal;
        var nivelBateria = 85;
        var intensidadeSinal = -60;
        var dadosAdicionais = "{\"extra\": \"data\"}";
        var observacoes = "Leitura normal";

        // Act
        var leitura = new LeituraSensor(
            sensorId,
            valor,
            unidade,
            timestampLeitura,
            qualidade,
            nivelBateria,
            intensidadeSinal,
            dadosAdicionais,
            observacoes);

        // Assert
        leitura.Id.Should().NotBeEmpty();
        leitura.SensorId.Should().Be(sensorId);
        leitura.Valor.Should().Be(valor);
        leitura.Unidade.Should().Be(unidade);
        leitura.TimestampLeitura.Should().Be(timestampLeitura);
        leitura.TimestampRecebimento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        leitura.Qualidade.Should().Be(qualidade);
        leitura.NivelBateria.Should().Be(nivelBateria);
        leitura.IntensidadeSinal.Should().Be(intensidadeSinal);
        leitura.DadosAdicionais.Should().Be(dadosAdicionais);
        leitura.Observacoes.Should().Be(observacoes);
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoUnidadeInvalida(string? unidadeInvalida)
    {
        // Arrange & Act
        var act = () => new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            unidadeInvalida!,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unidade*");
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoTimestampFuturo()
    {
        // Arrange
        var timestampFuturo = DateTime.UtcNow.AddMinutes(10);

        // Act
        var act = () => new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            timestampFuturo);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Timestamp*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Construtor_DeveLancarExcecao_QuandoNivelBateriaInvalido(int nivelInvalido)
    {
        // Arrange & Act
        var act = () => new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            nivelInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*bateria*");
    }

    [Fact]
    public void MarcarComoSuspeita_DeveAlterarQualidadeParaSuspeita()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        var motivo = "Valor fora do padrão esperado";

        // Act
        leitura.MarcarComoSuspeita(motivo);

        // Assert
        leitura.Qualidade.Should().Be(QualidadeLeitura.Suspeita);
        leitura.Observacoes.Should().Be(motivo);
    }

    [Fact]
    public void MarcarComoInvalida_DeveAlterarQualidadeParaInvalida()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        var motivo = "Sensor com defeito";

        // Act
        leitura.MarcarComoInvalida(motivo);

        // Assert
        leitura.Qualidade.Should().Be(QualidadeLeitura.Invalida);
        leitura.Observacoes.Should().Be(motivo);
    }

    [Fact]
    public void MarcarComoCalibrada_DeveAlterarQualidadeParaCalibrada()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        var motivo = "Leitura após calibração";

        // Act
        leitura.MarcarComoCalibrada(motivo);

        // Assert
        leitura.Qualidade.Should().Be(QualidadeLeitura.Calibrada);
        leitura.Observacoes.Should().Be(motivo);
    }

    [Fact]
    public void BateriaBaixa_DeveRetornarTrue_QuandoNivelAbaixoDe20()
    {
        // Arrange
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            15);

        // Act
        var bateriaBaixa = leitura.BateriaBaixa();

        // Assert
        bateriaBaixa.Should().BeTrue();
    }

    [Fact]
    public void BateriaBaixa_DeveRetornarFalse_QuandoNivelAcimaDe20()
    {
        // Arrange
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            80);

        // Act
        var bateriaBaixa = leitura.BateriaBaixa();

        // Assert
        bateriaBaixa.Should().BeFalse();
    }

    [Fact]
    public void BateriaBaixa_DeveRetornarFalse_QuandoNivelBateriaNulo()
    {
        // Arrange
        var leitura = CriarLeituraValida();

        // Act
        var bateriaBaixa = leitura.BateriaBaixa();

        // Assert
        bateriaBaixa.Should().BeFalse();
    }

    [Fact]
    public void SinalFraco_DeveRetornarTrue_QuandoRSSIAbaixoMenos80()
    {
        // Arrange
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            null,
            -90);

        // Act
        var sinalFraco = leitura.SinalFraco();

        // Assert
        sinalFraco.Should().BeTrue();
    }

    [Fact]
    public void SinalFraco_DeveRetornarFalse_QuandoRSSIAcimaMenos80()
    {
        // Arrange
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            null,
            -50);

        // Act
        var sinalFraco = leitura.SinalFraco();

        // Assert
        sinalFraco.Should().BeFalse();
    }

    [Fact]
    public void SinalFraco_DeveRetornarFalse_QuandoIntensidadeSinalNula()
    {
        // Arrange
        var leitura = CriarLeituraValida();

        // Act
        var sinalFraco = leitura.SinalFraco();

        // Assert
        sinalFraco.Should().BeFalse();
    }

    [Fact]
    public void LatenciaRecebimento_DeveCalcularDiferencaCorreta()
    {
        // Arrange
        var timestampLeitura = DateTime.UtcNow.AddMinutes(-5);
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            timestampLeitura);

        // Act
        var latencia = leitura.LatenciaRecebimento();

        // Assert
        latencia.TotalMinutes.Should().BeGreaterThan(4).And.BeLessThan(6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public void Construtor_DeveAceitarNiveisBateriaValidos(int nivelBateria)
    {
        // Arrange & Act
        var leitura = new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow,
            QualidadeLeitura.Normal,
            nivelBateria);

        // Assert
        leitura.NivelBateria.Should().Be(nivelBateria);
    }

    private static LeituraSensor CriarLeituraValida()
    {
        return new LeituraSensor(
            Guid.NewGuid(),
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-1));
    }
}
