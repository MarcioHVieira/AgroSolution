using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.IngestaoDados.Test.Domain.Entities;

public class SensorTests
{
    [Fact]
    public void Construtor_DeveCriarSensorComDadosValidos()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var deviceId = "SENSOR-001";
        var nome = "Sensor de Temperatura 1";
        var tipo = TipoSensor.Temperatura;
        var intervalo = 15;
        var talhaoId = Guid.NewGuid();
        var fabricante = "Fabricante X";
        var modelo = "Modelo Y";
        var latitude = -23.5505m;
        var longitude = -46.6333m;
        var altitude = 750m;
        var observacoes = "Sensor principal";

        // Act
        var sensor = new Sensor(
            propriedadeId,
            deviceId,
            nome,
            tipo,
            intervalo,
            talhaoId,
            fabricante,
            modelo,
            latitude,
            longitude,
            altitude,
            observacoes);

        // Assert
        sensor.Id.Should().NotBeEmpty();
        sensor.PropriedadeId.Should().Be(propriedadeId);
        sensor.TalhaoId.Should().Be(talhaoId);
        sensor.DeviceId.Should().Be(deviceId.ToUpperInvariant());
        sensor.Nome.Should().Be(nome);
        sensor.Tipo.Should().Be(tipo);
        sensor.Fabricante.Should().Be(fabricante);
        sensor.Modelo.Should().Be(modelo);
        sensor.Latitude.Should().Be(latitude);
        sensor.Longitude.Should().Be(longitude);
        sensor.Altitude.Should().Be(altitude);
        sensor.IntervaloLeituraMinutos.Should().Be(intervalo);
        sensor.Status.Should().Be(StatusSensor.Ativo);
        sensor.Observacoes.Should().Be(observacoes);
        sensor.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sensor.DataAtualizacao.Should().BeNull();
        sensor.UltimaLeitura.Should().BeNull();
        sensor.UltimaCalibracao.Should().BeNull();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoDeviceIdInvalido(string? deviceIdInvalido)
    {
        // Arrange & Act
        var act = () => new Sensor(
            Guid.NewGuid(),
            deviceIdInvalido!,
            "Sensor Teste",
            TipoSensor.Temperatura);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device ID*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange & Act
        var act = () => new Sensor(
            Guid.NewGuid(),
            "SENSOR-001",
            nomeInvalido!,
            TipoSensor.Temperatura);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Construtor_DeveLancarExcecao_QuandoIntervaloInvalido(int intervaloInvalido)
    {
        // Arrange & Act
        var act = () => new Sensor(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor Teste",
            TipoSensor.Temperatura,
            intervaloInvalido);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Intervalo de leitura*");
    }

    [Fact]
    public void Construtor_DeveConverterDeviceIdParaMaiuscula()
    {
        // Arrange
        var deviceId = "sensor-001";

        // Act
        var sensor = new Sensor(
            Guid.NewGuid(),
            deviceId,
            "Sensor Teste",
            TipoSensor.Temperatura);

        // Assert
        sensor.DeviceId.Should().Be("SENSOR-001");
    }

    [Fact]
    public void Atualizar_DeveAtualizarSensorComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var novoNome = "Sensor Atualizado";
        var novoIntervalo = 30;
        var novoTalhaoId = Guid.NewGuid();
        var novaLatitude = -22.9068m;
        var novaLongitude = -43.1729m;

        // Act
        sensor.Atualizar(
            novoNome,
            novoIntervalo,
            novoTalhaoId,
            "Novo Fabricante",
            "Novo Modelo",
            novaLatitude,
            novaLongitude,
            800m,
            "Novas observações");

        // Assert
        sensor.Nome.Should().Be(novoNome);
        sensor.IntervaloLeituraMinutos.Should().Be(novoIntervalo);
        sensor.TalhaoId.Should().Be(novoTalhaoId);
        sensor.Latitude.Should().Be(novaLatitude);
        sensor.Longitude.Should().Be(novaLongitude);
        sensor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var sensor = CriarSensorValido();

        // Act
        var act = () => sensor.Atualizar(nomeInvalido!, 15);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Fact]
    public void AtualizarStatus_DeveAlterarStatusComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var novoStatus = StatusSensor.EmManutencao;

        // Act
        sensor.AtualizarStatus(novoStatus);

        // Assert
        sensor.Status.Should().Be(novoStatus);
        sensor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarLeitura_DeveAtualizarUltimaLeitura()
    {
        // Arrange
        var sensor = CriarSensorValido();

        // Act
        sensor.RegistrarLeitura();

        // Assert
        sensor.UltimaLeitura.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sensor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarCalibracao_DeveAtualizarUltimaCalibracaoEStatus()
    {
        // Arrange
        var sensor = CriarSensorValido();
        sensor.AtualizarStatus(StatusSensor.AguardandoCalibracao);

        // Act
        sensor.RegistrarCalibracao();

        // Assert
        sensor.UltimaCalibracao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sensor.Status.Should().Be(StatusSensor.Ativo);
        sensor.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void EstaAtivo_DeveRetornarTrue_QuandoStatusAtivo()
    {
        // Arrange
        var sensor = CriarSensorValido();

        // Act
        var estaAtivo = sensor.EstaAtivo();

        // Assert
        estaAtivo.Should().BeTrue();
    }

    [Fact]
    public void EstaAtivo_DeveRetornarFalse_QuandoStatusInativo()
    {
        // Arrange
        var sensor = CriarSensorValido();
        sensor.AtualizarStatus(StatusSensor.Inativo);

        // Act
        var estaAtivo = sensor.EstaAtivo();

        // Assert
        estaAtivo.Should().BeFalse();
    }

    [Fact]
    public void PrecisaCalibracao_DeveRetornarTrue_QuandoNuncaCalibrado()
    {
        // Arrange
        var sensor = CriarSensorValido();

        // Act
        var precisaCalibracao = sensor.PrecisaCalibracao();

        // Assert
        precisaCalibracao.Should().BeTrue();
    }

    [Fact]
    public void PrecisaCalibracao_DeveRetornarFalse_QuandoCalibradoRecentemente()
    {
        // Arrange
        var sensor = CriarSensorValido();
        sensor.RegistrarCalibracao();

        // Act
        var precisaCalibracao = sensor.PrecisaCalibracao();

        // Assert
        precisaCalibracao.Should().BeFalse();
    }

    [Fact]
    public void PrecisaCalibracao_DeveRetornarTrue_QuandoCalibradoHaMaisDe90Dias()
    {
        // Arrange
        var sensor = CriarSensorValido();
        sensor.RegistrarCalibracao();

        // Simula calibração há 91 dias usando reflection
        var ultimaCalibracaoProperty = typeof(Sensor).GetProperty("UltimaCalibracao");
        ultimaCalibracaoProperty?.SetValue(sensor, DateTime.UtcNow.AddDays(-91));

        // Act
        var precisaCalibracao = sensor.PrecisaCalibracao();

        // Assert
        precisaCalibracao.Should().BeTrue();
    }

    private static Sensor CriarSensorValido()
    {
        return new Sensor(
            Guid.NewGuid(),
            "SENSOR-TEST-001",
            "Sensor de Teste",
            TipoSensor.Temperatura,
            15);
    }
}
