using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using FluentAssertions;

namespace ProcessamentoDados.Test.Domain.Entities;

public class AgregacaoDadosTests
{
    [Fact]
    public void Construtor_DeveCriarAgregacaoComDadosValidos()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var deviceId = "SENSOR-001";
        var propriedadeId = Guid.NewGuid();
        var tipoSensor = TipoSensor.Temperatura;
        var tipoAgregacao = TipoAgregacao.Horaria;
        var periodoInicio = DateTime.UtcNow.Date.AddHours(10);
        var periodoFim = periodoInicio.AddHours(1);
        var totalLeituras = 60;
        var unidade = "°C";
        var talhaoId = Guid.NewGuid();
        var valorMinimo = 20.5m;
        var valorMaximo = 28.3m;
        var valorMedio = 24.5m;
        var desvioPadrao = 2.1m;
        var leiturasNormais = 55;
        var leiturasSuspeitas = 3;
        var leiturasInvalidas = 2;

        // Act
        var agregacao = new AgregacaoDados(
            sensorId,
            deviceId,
            propriedadeId,
            tipoSensor,
            tipoAgregacao,
            periodoInicio,
            periodoFim,
            totalLeituras,
            unidade,
            talhaoId,
            valorMinimo,
            valorMaximo,
            valorMedio,
            desvioPadrao,
            leiturasNormais,
            leiturasSuspeitas,
            leiturasInvalidas);

        // Assert
        agregacao.Id.Should().NotBeEmpty();
        agregacao.SensorId.Should().Be(sensorId);
        agregacao.DeviceId.Should().Be(deviceId.ToUpperInvariant());
        agregacao.PropriedadeId.Should().Be(propriedadeId);
        agregacao.TalhaoId.Should().Be(talhaoId);
        agregacao.TipoSensor.Should().Be(tipoSensor);
        agregacao.TipoAgregacao.Should().Be(tipoAgregacao);
        agregacao.PeriodoInicio.Should().Be(periodoInicio);
        agregacao.PeriodoFim.Should().Be(periodoFim);
        agregacao.TotalLeituras.Should().Be(totalLeituras);
        agregacao.Unidade.Should().Be(unidade);
        agregacao.ValorMinimo.Should().Be(valorMinimo);
        agregacao.ValorMaximo.Should().Be(valorMaximo);
        agregacao.ValorMedio.Should().Be(valorMedio);
        agregacao.DesvioPadrao.Should().Be(desvioPadrao);
        agregacao.LeiturasNormais.Should().Be(leiturasNormais);
        agregacao.LeiturasSuspeitas.Should().Be(leiturasSuspeitas);
        agregacao.LeiturasInvalidas.Should().Be(leiturasInvalidas);
        agregacao.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoDeviceIdInvalido(string? deviceIdInvalido)
    {
        // Arrange & Act
        var act = () => new AgregacaoDados(
            Guid.NewGuid(),
            deviceIdInvalido!,
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10,
            "°C");

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
        var act = () => new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10,
            unidadeInvalida!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unidade*");
    }

    [Fact]
    public void Construtor_DeveLancarExcecao_QuandoPeriodoFimAnteriorAoInicio()
    {
        // Arrange
        var periodoInicio = DateTime.UtcNow;
        var periodoFim = periodoInicio.AddHours(-1); // Período fim anterior ao início

        // Act
        var act = () => new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            periodoInicio,
            periodoFim,
            10,
            "°C");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*período fim*");
    }

    [Fact]
    public void Construtor_DeveConverterDeviceIdParaMaiuscula()
    {
        // Arrange
        var deviceId = "sensor-001";

        // Act
        var agregacao = new AgregacaoDados(
            Guid.NewGuid(),
            deviceId,
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10,
            "°C");

        // Assert
        agregacao.DeviceId.Should().Be("SENSOR-001");
    }

    [Theory]
    [InlineData(TipoAgregacao.Horaria)]
    [InlineData(TipoAgregacao.Diaria)]
    [InlineData(TipoAgregacao.Semanal)]
    [InlineData(TipoAgregacao.Mensal)]
    public void Construtor_DeveAceitarTodosTiposDeAgregacao(TipoAgregacao tipo)
    {
        // Arrange & Act
        var agregacao = new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            tipo,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10,
            "°C");

        // Assert
        agregacao.TipoAgregacao.Should().Be(tipo);
    }

    [Fact]
    public void Construtor_DeveAceitarValoresNulos()
    {
        // Arrange & Act
        var agregacao = new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            10,
            "°C");

        // Assert
        agregacao.TalhaoId.Should().BeNull();
        agregacao.ValorMinimo.Should().BeNull();
        agregacao.ValorMaximo.Should().BeNull();
        agregacao.ValorMedio.Should().BeNull();
        agregacao.DesvioPadrao.Should().BeNull();
    }

    [Fact]
    public void Construtor_DeveCalcularEstatisticasCorretamente()
    {
        // Arrange
        var totalLeituras = 100;
        var leiturasNormais = 90;
        var leiturasSuspeitas = 7;
        var leiturasInvalidas = 3;

        // Act
        var agregacao = new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            totalLeituras,
            "°C",
            null,
            20m,
            30m,
            25m,
            2.5m,
            leiturasNormais,
            leiturasSuspeitas,
            leiturasInvalidas);

        // Assert
        agregacao.TotalLeituras.Should().Be(totalLeituras);
        (agregacao.LeiturasNormais + agregacao.LeiturasSuspeitas + agregacao.LeiturasInvalidas)
            .Should().Be(totalLeituras);
    }
}
