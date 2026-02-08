using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Events;
using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.ProcessamentoDados.Application.Services;
using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ProcessamentoDados.Test.Application.Services;

public class ProcessamentoServiceTests
{
    private readonly Mock<ILeituraProcessadaRepository> _repositoryMock;
    private readonly Mock<IAgregacaoService> _agregacaoServiceMock;
    private readonly Mock<IRabbitMQPublisher> _publisherMock;
    private readonly Mock<ILogger<ProcessamentoService>> _loggerMock;
    private readonly ProcessamentoService _service;

    public ProcessamentoServiceTests()
    {
        _repositoryMock = new Mock<ILeituraProcessadaRepository>();
        _agregacaoServiceMock = new Mock<IAgregacaoService>();
        _publisherMock = new Mock<IRabbitMQPublisher>();
        _loggerMock = new Mock<ILogger<ProcessamentoService>>();
        _service = new ProcessamentoService(
            _repositoryMock.Object,
            _agregacaoServiceMock.Object,
            _publisherMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_DeveProcessarLeituraComSucesso()
    {
        // Arrange
        var evento = CriarEventoValido();

        _repositoryMock
            .Setup(x => x.ObterPorLeituraOrigemIdAsync(evento.Id))
            .ReturnsAsync((LeituraProcessada?)null);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<LeituraProcessada>()))
            .Returns(Task.CompletedTask);

        _agregacaoServiceMock
            .Setup(x => x.AgregacaoExisteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(false);

        _agregacaoServiceMock
            .Setup(x => x.GerarAgregacaoHorariaAsync(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarLeituraAsync(evento);

        // Assert
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<LeituraProcessada>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarLeituraAsync_NaoDeveProcessar_QuandoJaProcessada()
    {
        // Arrange
        var evento = CriarEventoValido();
        var leituraExistente = CriarLeituraValida();

        _repositoryMock
            .Setup(x => x.ObterPorLeituraOrigemIdAsync(evento.Id))
            .ReturnsAsync(leituraExistente);

        // Act
        await _service.ProcessarLeituraAsync(evento);

        // Assert
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<LeituraProcessada>()), Times.Never);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarLeitura_QuandoEncontrada()
    {
        // Arrange
        var leitura = CriarLeituraValida();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(leitura.Id))
            .ReturnsAsync(leitura);

        // Act
        var resultado = await _service.ObterPorIdAsync(leitura.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(leitura.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id))
            .ReturnsAsync((LeituraProcessada?)null);

        // Act
        var resultado = await _service.ObterPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterPorLeituraOrigemIdAsync_DeveRetornarLeitura_QuandoEncontrada()
    {
        // Arrange
        var leitura = CriarLeituraValida();

        _repositoryMock
            .Setup(x => x.ObterPorLeituraOrigemIdAsync(leitura.LeituraOrigemId))
            .ReturnsAsync(leitura);

        // Act
        var resultado = await _service.ObterPorLeituraOrigemIdAsync(leitura.LeituraOrigemId);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.LeituraOrigemId.Should().Be(leitura.LeituraOrigemId);
    }

    [Fact]
    public async Task ConsultarLeiturasAsync_DeveRetornarLeiturasPorSensor()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var filtros = new ConsultarLeiturasDto(
            sensorId,
            null,
            null,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            null,
            null,
            null,
            null);

        var leituras = new List<LeituraProcessada>
        {
            CriarLeituraValida(),
            CriarLeituraValida()
        };

        _repositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, filtros.DataInicio, filtros.DataFim))
            .ReturnsAsync(leituras);

        // Act
        var resultado = await _service.ConsultarLeiturasAsync(filtros);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ConsultarLeiturasAsync_DeveRetornarLeiturasPorPropriedade()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var filtros = new ConsultarLeiturasDto(
            null,
            propriedadeId,
            null,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            null,
            null,
            null,
            null);

        var leituras = new List<LeituraProcessada>
        {
            CriarLeituraValida()
        };

        _repositoryMock
            .Setup(x => x.ObterPorPropriedadeAsync(propriedadeId, filtros.DataInicio, filtros.DataFim))
            .ReturnsAsync(leituras);

        // Act
        var resultado = await _service.ConsultarLeiturasAsync(filtros);

        // Assert
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConsultarLeiturasAsync_DeveLancarExcecao_QuandoSemFiltrosObrigatorios()
    {
        // Arrange
        var filtros = new ConsultarLeiturasDto(
            null,
            null,
            null,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            null,
            null,
            null,
            null);

        // Act
        var act = async () => await _service.ConsultarLeiturasAsync(filtros);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SensorId*PropriedadeId*TalhaoId*");
    }

    [Fact]
    public async Task ObterEstatisticasAsync_DeveRetornarEstatisticas()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.AddDays(-7);
        var dataFim = DateTime.UtcNow;

        _repositoryMock
            .Setup(x => x.ContarPorStatusAsync(StatusProcessamento.Processado))
            .ReturnsAsync(95);

        _repositoryMock
            .Setup(x => x.ContarPorStatusAsync(StatusProcessamento.Falha))
            .ReturnsAsync(5);

        // Act
        var resultado = await _service.ObterEstatisticasAsync(dataInicio, dataFim);

        // Assert
        resultado.Should().NotBeNull();
        resultado.LeiturasComSucesso.Should().Be(95);
        resultado.LeiturasComFalha.Should().Be(5);
        resultado.TaxaSucesso.Should().Be(95m);
    }

    [Fact]
    public async Task ReprocessarFalhasAsync_DeveReprocessarLeituras()
    {
        // Arrange
        var leituras = new List<LeituraProcessada>
        {
            CriarLeituraValida(),
            CriarLeituraValida()
        };

        foreach (var leitura in leituras)
        {
            leitura.MarcarComoFalha("Erro inicial");
        }

        _repositoryMock
            .Setup(x => x.ObterComFalhaAsync(It.IsAny<int>()))
            .ReturnsAsync(leituras);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<LeituraProcessada>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ReprocessarFalhasAsync(100);

        // Assert
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<LeituraProcessada>()), Times.Exactly(2));
        leituras.Should().AllSatisfy(l => l.Status.Should().Be(StatusProcessamento.Processado));
    }

    [Fact]
    public async Task ContarPorStatusAsync_DeveRetornarContagem()
    {
        // Arrange
        var status = "Processado";
        var contagem = 100;

        _repositoryMock
            .Setup(x => x.ContarPorStatusAsync(StatusProcessamento.Processado))
            .ReturnsAsync(contagem);

        // Act
        var resultado = await _service.ContarPorStatusAsync(status);

        // Assert
        resultado.Should().Be(contagem);
    }

    [Fact]
    public async Task ContarPorStatusAsync_DeveLancarExcecao_QuandoStatusInvalido()
    {
        // Arrange
        var statusInvalido = "StatusInvalido";

        // Act
        var act = async () => await _service.ContarPorStatusAsync(statusInvalido);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{statusInvalido}*");
    }

    private static LeituraRecebidaEvent CriarEventoValido()
    {
        return new LeituraRecebidaEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "SENSOR-001",
            Guid.NewGuid(),
            null,
            TipoSensor.Temperatura,
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-10),
            DateTime.UtcNow.AddMinutes(-5),
            QualidadeLeitura.Normal,
            85,
            -60,
            false,
            false,
            TimeSpan.FromMinutes(5),
            null);
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
