using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Services;
using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.IngestaoDados.Test.Application.Services;

public class SensorServiceTests
{
    private readonly Mock<ISensorRepository> _repositoryMock;
    private readonly Mock<ILogger<SensorService>> _loggerMock;
    private readonly SensorService _service;

    public SensorServiceTests()
    {
        _repositoryMock = new Mock<ISensorRepository>();
        _loggerMock = new Mock<ILogger<SensorService>>();
        _service = new SensorService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarSensorComSucesso()
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        _repositoryMock
            .Setup(x => x.DeviceIdExisteAsync(dto.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.DeviceId.Should().Be(dto.DeviceId.ToUpperInvariant());
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoDeviceIdJaExiste()
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        _repositoryMock
            .Setup(x => x.DeviceIdExisteAsync(dto.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _service.CriarAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DeviceId*");
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarSensor_QuandoEncontrado()
    {
        // Arrange
        var sensor = CriarSensorValido();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var resultado = await _service.ObterPorIdAsync(sensor.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(sensor.Id);
        resultado.Nome.Should().Be(sensor.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var act = async () => await _service.ObterPorIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task ObterPorDeviceIdAsync_DeveRetornarSensor_QuandoEncontrado()
    {
        // Arrange
        var sensor = CriarSensorValido();

        _repositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(sensor.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var resultado = await _service.ObterPorDeviceIdAsync(sensor.DeviceId);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DeviceId.Should().Be(sensor.DeviceId);
    }

    [Fact]
    public async Task ObterPorDeviceIdAsync_DeveLancarExcecao_QuandoNaoEncontrado()
    {
        // Arrange
        var deviceId = "SENSOR-999";

        _repositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var act = async () => await _service.ObterPorDeviceIdAsync(deviceId);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{deviceId}*");
    }

    [Fact]
    public async Task ObterPorPropriedadeAsync_DeveRetornarSensoresDaPropriedade()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var sensores = new List<Sensor>
        {
            CriarSensorValido(),
            CriarSensorValido()
        };

        _repositoryMock
            .Setup(x => x.ObterPorPropriedadeIdAsync(propriedadeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensores);

        // Act
        var resultado = await _service.ObterPorPropriedadeAsync(propriedadeId);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterPorTalhaoAsync_DeveRetornarSensoresDoTalhao()
    {
        // Arrange
        var talhaoId = Guid.NewGuid();
        var sensores = new List<Sensor>
        {
            CriarSensorValido()
        };

        _repositoryMock
            .Setup(x => x.ObterPorTalhaoIdAsync(talhaoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensores);

        // Act
        var resultado = await _service.ObterPorTalhaoAsync(talhaoId);

        // Assert
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterPorTipoAsync_DeveRetornarSensoresDoTipo()
    {
        // Arrange
        var tipo = TipoSensor.Temperatura;
        var sensores = new List<Sensor>
        {
            CriarSensorValido()
        };

        _repositoryMock
            .Setup(x => x.ObterPorTipoAsync(tipo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensores);

        // Act
        var resultado = await _service.ObterPorTipoAsync(tipo);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.Should().AllSatisfy(s => s.Tipo.Should().Be(tipo));
    }

    [Fact]
    public async Task ObterAtivosPorPropriedadeAsync_DeveRetornarApenasSensoresAtivos()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var sensores = new List<Sensor>
        {
            CriarSensorValido()
        };

        _repositoryMock
            .Setup(x => x.ObterAtivosPorPropriedadeAsync(propriedadeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensores);

        // Act
        var resultado = await _service.ObterAtivosPorPropriedadeAsync(propriedadeId);

        // Assert
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarSensorComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var dto = new AtualizarSensorDto(
            "Sensor Atualizado",
            30,
            Guid.NewGuid(),
            "Novo Fabricante",
            "Novo Modelo",
            -22.9068m,
            -43.1729m,
            800m,
            "Novas observações");

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AtualizarAsync(sensor.Id, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.IntervaloLeituraMinutos.Should().Be(dto.IntervaloLeituraMinutos);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarExcecao_QuandoSensorNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AtualizarSensorDto("Sensor Teste", 15);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var act = async () => await _service.AtualizarAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveAlterarStatusComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var novoStatus = StatusSensor.EmManutencao;

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AtualizarStatusAsync(sensor.Id, novoStatus);

        // Assert
        sensor.Status.Should().Be(novoStatus);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarCalibracaoAsync_DeveRegistrarCalibracaoComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RegistrarCalibracaoAsync(sensor.Id);

        // Assert
        sensor.UltimaCalibracao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        sensor.Status.Should().Be(StatusSensor.Ativo);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverSensorComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _repositoryMock
            .Setup(x => x.RemoverAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoverAsync(sensor.Id);

        // Assert
        _repositoryMock.Verify(x => x.RemoverAsync(sensor.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoSensorNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var act = async () => await _service.RemoverAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
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
