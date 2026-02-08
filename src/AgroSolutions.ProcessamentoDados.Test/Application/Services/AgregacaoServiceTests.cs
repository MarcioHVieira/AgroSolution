using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Services;
using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace ProcessamentoDados.Test.Application.Services;

public class AgregacaoServiceTests
{
    private readonly Mock<IAgregacaoDadosRepository> _agregacaoRepositoryMock;
    private readonly Mock<ILeituraProcessadaRepository> _leituraRepositoryMock;
    private readonly Mock<ILogger<AgregacaoService>> _loggerMock;
    private readonly AgregacaoService _service;

    public AgregacaoServiceTests()
    {
        _agregacaoRepositoryMock = new Mock<IAgregacaoDadosRepository>();
        _leituraRepositoryMock = new Mock<ILeituraProcessadaRepository>();
        _loggerMock = new Mock<ILogger<AgregacaoService>>();
        _service = new AgregacaoService(
            _agregacaoRepositoryMock.Object,
            _leituraRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GerarAgregacaoHorariaAsync_DeveGerarAgregacaoComSucesso()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var hora = DateTime.UtcNow.Date.AddHours(10);
        var leituras = CriarListaLeituras(sensorId, 60);

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Horaria, It.IsAny<DateTime>()))
            .ReturnsAsync((AgregacaoDados?)null);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(leituras);

        _agregacaoRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.GerarAgregacaoHorariaAsync(sensorId, hora);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Once);
    }

    [Fact]
    public async Task GerarAgregacaoHorariaAsync_NaoDeveGerar_QuandoJaExiste()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var hora = DateTime.UtcNow.Date.AddHours(10);
        var agregacaoExistente = CriarAgregacaoValida();

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Horaria, It.IsAny<DateTime>()))
            .ReturnsAsync(agregacaoExistente);

        // Act
        await _service.GerarAgregacaoHorariaAsync(sensorId, hora);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Never);
    }

    [Fact]
    public async Task GerarAgregacaoHorariaAsync_NaoDeveGerar_QuandoSemLeituras()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var hora = DateTime.UtcNow.Date.AddHours(10);

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Horaria, It.IsAny<DateTime>()))
            .ReturnsAsync((AgregacaoDados?)null);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<LeituraProcessada>());

        // Act
        await _service.GerarAgregacaoHorariaAsync(sensorId, hora);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Never);
    }

    [Fact]
    public async Task GerarAgregacaoDiariaAsync_DeveGerarAgregacaoComSucesso()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var dia = DateTime.UtcNow.Date;
        var leituras = CriarListaLeituras(sensorId, 1440); // 24 horas * 60 minutos

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Diaria, It.IsAny<DateTime>()))
            .ReturnsAsync((AgregacaoDados?)null);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(leituras);

        _agregacaoRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.GerarAgregacaoDiariaAsync(sensorId, dia);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Once);
    }

    [Fact]
    public async Task GerarAgregacaoSemanalAsync_DeveGerarAgregacaoComSucesso()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var semana = DateTime.UtcNow.Date;
        var leituras = CriarListaLeituras(sensorId, 10080); // 7 dias * 24 horas * 60 minutos

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Semanal, It.IsAny<DateTime>()))
            .ReturnsAsync((AgregacaoDados?)null);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(leituras);

        _agregacaoRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.GerarAgregacaoSemanalAsync(sensorId, semana);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Once);
    }

    [Fact]
    public async Task GerarAgregacaoMensalAsync_DeveGerarAgregacaoComSucesso()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var mes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var leituras = CriarListaLeituras(sensorId, 43200); // 30 dias * 24 horas * 60 minutos

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Mensal, It.IsAny<DateTime>()))
            .ReturnsAsync((AgregacaoDados?)null);

        _leituraRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(sensorId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(leituras);

        _agregacaoRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.GerarAgregacaoMensalAsync(sensorId, mes);

        // Assert
        _agregacaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<AgregacaoDados>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarAgregacao_QuandoEncontrada()
    {
        // Arrange
        var agregacao = CriarAgregacaoValida();

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(agregacao.Id))
            .ReturnsAsync(agregacao);

        // Act
        var resultado = await _service.ObterPorIdAsync(agregacao.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(agregacao.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(id))
            .ReturnsAsync((AgregacaoDados?)null);

        // Act
        var resultado = await _service.ObterPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ConsultarAgregacoesAsync_DeveRetornarAgregacoesPorSensor()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.AddDays(-7);
        var dataFim = DateTime.UtcNow;
        var filtros = new ConsultarAgregacoesDto(
            sensorId,
            null,
            null,
            TipoAgregacao.Horaria,
            dataInicio,
            dataFim);

        var agregacoes = new List<AgregacaoDados>
        {
            CriarAgregacaoValida(),
            CriarAgregacaoValida()
        };

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorSensorAsync(
                sensorId,
                TipoAgregacao.Horaria,
                dataInicio,
                dataFim))
            .ReturnsAsync(agregacoes);

        // Act
        var resultado = await _service.ConsultarAgregacoesAsync(filtros);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ConsultarAgregacoesAsync_DeveRetornarAgregacoesPorPropriedade()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var dataInicio = DateTime.UtcNow.AddDays(-30);
        var dataFim = DateTime.UtcNow;
        var filtros = new ConsultarAgregacoesDto(
            null,
            propriedadeId,
            null,
            TipoAgregacao.Diaria,
            dataInicio,
            dataFim);

        var agregacoes = new List<AgregacaoDados>
        {
            CriarAgregacaoValida()
        };

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPropriedadeAsync(
                propriedadeId,
                TipoAgregacao.Diaria,
                dataInicio,
                dataFim))
            .ReturnsAsync(agregacoes);

        // Act
        var resultado = await _service.ConsultarAgregacoesAsync(filtros);

        // Assert
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ConsultarAgregacoesAsync_DeveLancarExcecao_QuandoSemFiltrosObrigatorios()
    {
        // Arrange
        var dataInicio = DateTime.UtcNow.AddDays(-7);
        var dataFim = DateTime.UtcNow;
        var filtros = new ConsultarAgregacoesDto(
            null,
            null,
            null,
            TipoAgregacao.Horaria,
            dataInicio,
            dataFim);

        // Act
        var act = async () => await _service.ConsultarAgregacoesAsync(filtros);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SensorId*PropriedadeId*");
    }

    [Fact]
    public async Task AgregacaoExisteAsync_DeveRetornarTrue_QuandoExiste()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var periodo = DateTime.UtcNow.Date.AddHours(10);
        var agregacao = CriarAgregacaoValida();

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Horaria, periodo))
            .ReturnsAsync(agregacao);

        // Act
        var resultado = await _service.AgregacaoExisteAsync(sensorId, "Horaria", periodo);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task AgregacaoExisteAsync_DeveRetornarFalse_QuandoNaoExiste()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var periodo = DateTime.UtcNow.Date.AddHours(10);

        _agregacaoRepositoryMock
            .Setup(x => x.ObterPorPeriodoAsync(sensorId, TipoAgregacao.Horaria, periodo))
            .ReturnsAsync((AgregacaoDados?)null);

        // Act
        var resultado = await _service.AgregacaoExisteAsync(sensorId, "Horaria", periodo);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task AgregacaoExisteAsync_DeveRetornarFalse_QuandoTipoInvalido()
    {
        // Arrange
        var sensorId = Guid.NewGuid();
        var periodo = DateTime.UtcNow.Date.AddHours(10);

        // Act
        var resultado = await _service.AgregacaoExisteAsync(sensorId, "TipoInvalido", periodo);

        // Assert
        resultado.Should().BeFalse();
    }

    private static List<LeituraProcessada> CriarListaLeituras(Guid sensorId, int quantidade)
    {
        var leituras = new List<LeituraProcessada>();
        var random = new Random();
        var baseTimestamp = DateTime.UtcNow.AddHours(-1);

        for (int i = 0; i < quantidade; i++)
        {
            var valor = 20m + (decimal)(random.NextDouble() * 10); // 20 a 30
            var leitura = new LeituraProcessada(
                Guid.NewGuid(),
                sensorId,
                "SENSOR-TEST-001",
                Guid.NewGuid(),
                TipoSensor.Temperatura,
                valor,
                "°C",
                baseTimestamp.AddMinutes(i),
                baseTimestamp.AddMinutes(i).AddSeconds(5),
                QualidadeLeitura.Normal);

            leituras.Add(leitura);
        }

        return leituras;
    }

    private static AgregacaoDados CriarAgregacaoValida()
    {
        return new AgregacaoDados(
            Guid.NewGuid(),
            "SENSOR-TEST-001",
            Guid.NewGuid(),
            TipoSensor.Temperatura,
            TipoAgregacao.Horaria,
            DateTime.UtcNow.Date.AddHours(10),
            DateTime.UtcNow.Date.AddHours(11),
            60,
            "°C",
            null,
            20.5m,
            28.3m,
            24.5m,
            2.1m,
            55,
            3,
            2);
    }
}
