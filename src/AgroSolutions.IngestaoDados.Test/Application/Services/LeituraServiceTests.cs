using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Events;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.IngestaoDados.Application.Services;
using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.IngestaoDados.Test.Application.Services;

public class LeituraServiceTests
{
    private readonly Mock<ILeituraSensorRepository> _leituraRepositoryMock;
    private readonly Mock<ISensorRepository> _sensorRepositoryMock;
    private readonly Mock<IMensageriaService> _mensageriaServiceMock;
    private readonly Mock<ILogger<LeituraService>> _loggerMock;
    private readonly LeituraService _service;

    public LeituraServiceTests()
    {
        _leituraRepositoryMock = new Mock<ILeituraSensorRepository>();
        _sensorRepositoryMock = new Mock<ISensorRepository>();
        _mensageriaServiceMock = new Mock<IMensageriaService>();
        _loggerMock = new Mock<ILogger<LeituraService>>();
        _service = new LeituraService(
            _leituraRepositoryMock.Object,
            _sensorRepositoryMock.Object,
            _mensageriaServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegistrarLeituraAsync_DeveRegistrarLeituraComSucesso()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var dto = new RegistrarLeituraDto(
            sensor.DeviceId,
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-1),
            85,
            -60,
            null);

        _sensorRepositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(sensor.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mensageriaServiceMock
            .Setup(x => x.PublicarLeituraRecebidaAsync(It.IsAny<LeituraRecebidaEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.RegistrarLeituraAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DeviceId.Should().Be(sensor.DeviceId);
        resultado.Valor.Should().Be(dto.Valor);
        resultado.Unidade.Should().Be(dto.Unidade);
        _leituraRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarLeituraAsync_DeveLancarExcecao_QuandoSensorNaoEncontrado()
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-999",
            25.5m,
            "°C",
            DateTime.UtcNow);

        _sensorRepositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(dto.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sensor?)null);

        // Act
        var act = async () => await _service.RegistrarLeituraAsync(dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{dto.DeviceId}*");
    }

    [Fact]
    public async Task RegistrarLeituraAsync_DeveLancarExcecao_QuandoSensorInativo()
    {
        // Arrange
        var sensor = CriarSensorValido();
        sensor.AtualizarStatus(StatusSensor.Inativo);
        
        var dto = new RegistrarLeituraDto(
            sensor.DeviceId,
            25.5m,
            "°C",
            DateTime.UtcNow);

        _sensorRepositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(sensor.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        // Act
        var act = async () => await _service.RegistrarLeituraAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não está ativo*");
    }

    [Fact]
    public async Task RegistrarLeituraAsync_DevePublicarEventoRabbitMQ()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var dto = new RegistrarLeituraDto(
            sensor.DeviceId,
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-1));

        _sensorRepositoryMock
            .Setup(x => x.ObterPorDeviceIdAsync(sensor.DeviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sensorRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Sensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RegistrarLeituraAsync(dto);

        // Assert
        _mensageriaServiceMock.Verify(
            x => x.PublicarLeituraRecebidaAsync(It.IsAny<LeituraRecebidaEvent>()),
            Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarLeitura_QuandoEncontrada()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var leitura = CriarLeituraValida(sensor);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(leitura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leitura);

        // Act
        var resultado = await _service.ObterPorIdAsync(leitura.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(leitura.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();

        _leituraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeituraSensor?)null);

        // Act
        var act = async () => await _service.ObterPorIdAsync(id);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task ObterPorSensorAsync_DeveRetornarLeituras()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var leituras = new List<LeituraSensor>
        {
            CriarLeituraValida(sensor),
            CriarLeituraValida(sensor)
        };

        _sensorRepositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorIdAsync(sensor.Id, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(leituras);

        // Act
        var resultado = await _service.ObterPorSensorAsync(sensor.Id);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterUltimaLeituraAsync_DeveRetornarUltimaLeitura()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var leitura = CriarLeituraValida(sensor);

        _sensorRepositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.ObterUltimaLeituraAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leitura);

        // Act
        var resultado = await _service.ObterUltimaLeituraAsync(sensor.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(leitura.Id);
    }

    [Fact]
    public async Task ObterUltimaLeituraAsync_DeveRetornarNull_QuandoSemLeituras()
    {
        // Arrange
        var sensor = CriarSensorValido();

        _sensorRepositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.ObterUltimaLeituraAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeituraSensor?)null);

        // Act
        var resultado = await _service.ObterUltimaLeituraAsync(sensor.Id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterEstatisticasAsync_DeveCalcularEstatisticasCorretamente()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var dataInicio = DateTime.UtcNow.AddDays(-7);
        var dataFim = DateTime.UtcNow;
        var leituras = new List<LeituraSensor>
        {
            new LeituraSensor(sensor.Id, 20m, "°C", DateTime.UtcNow.AddDays(-1)),
            new LeituraSensor(sensor.Id, 25m, "°C", DateTime.UtcNow.AddDays(-2)),
            new LeituraSensor(sensor.Id, 30m, "°C", DateTime.UtcNow.AddDays(-3))
        };

        _sensorRepositoryMock
            .Setup(x => x.ObterPorIdAsync(sensor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sensor);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensor.Id, dataInicio, dataFim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leituras);

        // Act
        var resultado = await _service.ObterEstatisticasAsync(sensor.Id, dataInicio, dataFim);

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalLeituras.Should().Be(3);
        resultado.ValorMinimo.Should().Be(20m);
        resultado.ValorMaximo.Should().Be(30m);
        resultado.ValorMedio.Should().Be(25m);
    }

    [Fact]
    public async Task MarcarComoSuspeitaAsync_DeveMarcarLeituraComoSuspeita()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var leitura = CriarLeituraValida(sensor);
        var motivo = "Valor fora do padrão";

        _leituraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(leitura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leitura);

        _leituraRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoSuspeitaAsync(leitura.Id, motivo);

        // Assert
        leitura.Qualidade.Should().Be(QualidadeLeitura.Suspeita);
        leitura.Observacoes.Should().Be(motivo);
        _leituraRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarComoInvalidaAsync_DeveMarcarLeituraComoInvalida()
    {
        // Arrange
        var sensor = CriarSensorValido();
        var leitura = CriarLeituraValida(sensor);
        var motivo = "Sensor com defeito";

        _leituraRepositoryMock
            .Setup(x => x.ObterPorIdAsync(leitura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(leitura);

        _leituraRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoInvalidaAsync(leitura.Id, motivo);

        // Assert
        leitura.Qualidade.Should().Be(QualidadeLeitura.Invalida);
        leitura.Observacoes.Should().Be(motivo);
        _leituraRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<LeituraSensor>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Sensor CriarSensorValido()
    {
        var sensor = new Sensor(
            Guid.NewGuid(),
            "SENSOR-TEST-001",
            "Sensor de Teste",
            TipoSensor.Temperatura,
            15);

        return sensor;
    }

    private static LeituraSensor CriarLeituraValida(Sensor sensor)
    {
        var leitura = new LeituraSensor(
            sensor.Id,
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-1));

        // Configurar propriedade de navegação usando reflection
        var sensorProperty = typeof(LeituraSensor).GetProperty("Sensor");
        sensorProperty?.SetValue(leitura, sensor);

        return leitura;
    }
}
