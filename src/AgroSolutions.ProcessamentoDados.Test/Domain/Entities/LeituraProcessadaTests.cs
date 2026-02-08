using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using FluentAssertions;

namespace ProcessamentoDados.Test.Domain.Entities;

public class LeituraProcessadaTests
{
    [Fact]
    public void Construtor_DeveCriarLeituraComDadosValidos()
    {
        // Arrange
        var leituraOrigemId = Guid.NewGuid();
        var sensorId = Guid.NewGuid();
        var deviceId = "SENSOR-001";
        var propriedadeId = Guid.NewGuid();
        var tipoSensor = TipoSensor.Temperatura;
        var valor = 25.5m;
        var unidade = "°C";
        var timestampLeitura = DateTime.UtcNow.AddMinutes(-10);
        var timestampRecebimento = DateTime.UtcNow.AddMinutes(-5);
        var qualidade = QualidadeLeitura.Normal;
        var talhaoId = Guid.NewGuid();
        var nivelBateria = 85;
        var intensidadeSinal = -60;
        var dadosAdicionais = "{\"extra\": \"data\"}";

        // Act
        var leitura = new LeituraProcessada(
            leituraOrigemId,
            sensorId,
            deviceId,
            propriedadeId,
            tipoSensor,
            valor,
            unidade,
            timestampLeitura,
            timestampRecebimento,
            qualidade,
            talhaoId,
            nivelBateria,
            intensidadeSinal,
            dadosAdicionais);

        // Assert
        leitura.Id.Should().NotBeEmpty();
        leitura.LeituraOrigemId.Should().Be(leituraOrigemId);
        leitura.SensorId.Should().Be(sensorId);
        leitura.DeviceId.Should().Be(deviceId.ToUpperInvariant());
        leitura.PropriedadeId.Should().Be(propriedadeId);
        leitura.TalhaoId.Should().Be(talhaoId);
        leitura.TipoSensor.Should().Be(tipoSensor);
        leitura.Valor.Should().Be(valor);
        leitura.Unidade.Should().Be(unidade);
        leitura.TimestampLeitura.Should().Be(timestampLeitura);
        leitura.TimestampRecebimento.Should().Be(timestampRecebimento);
        leitura.TimestampProcessamento.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        leitura.Qualidade.Should().Be(qualidade);
        leitura.NivelBateria.Should().Be(nivelBateria);
        leitura.IntensidadeSinal.Should().Be(intensidadeSinal);
        leitura.Status.Should().Be(StatusProcessamento.Processado);
        leitura.DadosAdicionais.Should().Be(dadosAdicionais);
        leitura.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        leitura.DataAtualizacao.Should().BeNull();
        leitura.MensagemErro.Should().BeNull();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoDeviceIdInvalido(string? deviceIdInvalido)
    {
        // Arrange & Act
        var act = () => new LeituraProcessada(
            Guid.NewGuid(),
            Guid.NewGuid(),
            deviceIdInvalido!,
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            25.5m,
            "°C",
            DateTime.UtcNow,
            DateTime.UtcNow,
            QualidadeLeitura.Normal);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device ID*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoUnidadeInvalida(string? unidadeInvalida)
    {
        // Arrange & Act
        var act = () => new LeituraProcessada(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            25.5m,
            unidadeInvalida!,
            DateTime.UtcNow,
            DateTime.UtcNow,
            QualidadeLeitura.Normal);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unidade*");
    }

    [Fact]
    public void Construtor_DeveConverterDeviceIdParaMaiuscula()
    {
        // Arrange
        var deviceId = "sensor-001";

        // Act
        var leitura = new LeituraProcessada(
            Guid.NewGuid(),
            Guid.NewGuid(),
            deviceId,
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            25.5m,
            "°C",
            DateTime.UtcNow,
            DateTime.UtcNow,
            QualidadeLeitura.Normal);

        // Assert
        leitura.DeviceId.Should().Be("SENSOR-001");
    }

    [Fact]
    public void MarcarComoFalha_DeveAlterarStatusParaFalha()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        var mensagemErro = "Erro ao processar dados";

        // Act
        leitura.MarcarComoFalha(mensagemErro);

        // Assert
        leitura.Status.Should().Be(StatusProcessamento.Falha);
        leitura.MensagemErro.Should().Be(mensagemErro);
        leitura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reprocessar_DeveAlterarStatusParaReprocessando()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        leitura.MarcarComoFalha("Erro inicial");

        // Act
        leitura.Reprocessar();

        // Assert
        leitura.Status.Should().Be(StatusProcessamento.Reprocessando);
        leitura.MensagemErro.Should().BeNull();
        leitura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarcarComoProcessado_DeveAlterarStatusParaProcessado()
    {
        // Arrange
        var leitura = CriarLeituraValida();
        leitura.MarcarComoFalha("Erro inicial");
        leitura.Reprocessar();

        // Act
        leitura.MarcarComoProcessado();

        // Assert
        leitura.Status.Should().Be(StatusProcessamento.Processado);
        leitura.MensagemErro.Should().BeNull();
        leitura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static LeituraProcessada CriarLeituraValida()
    {
        return new LeituraProcessada(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SENSOR-TEST-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-10),
            DateTime.UtcNow.AddMinutes(-5),
            QualidadeLeitura.Normal);
    }
}
