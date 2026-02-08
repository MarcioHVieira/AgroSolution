using AgroSolutions.Analise.Application.DTOs;
using AgroSolutions.Analise.Application.Services;
using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Analise.Test.Application.Services;

public class AlertaServiceTests
{
    private readonly Mock<IAlertaRepository> _repositoryMock;
    private readonly Mock<ILogger<AlertaService>> _loggerMock;
    private readonly AnaliseDbContext _context;
    private readonly AlertaService _service;

    public AlertaServiceTests()
    {
        _repositoryMock = new Mock<IAlertaRepository>();
        _loggerMock = new Mock<ILogger<AlertaService>>();
        
        var options = new DbContextOptionsBuilder<AnaliseDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        _context = new AnaliseDbContext(options);
        
        _service = new AlertaService(_repositoryMock.Object, _loggerMock.Object, _context);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarAlerta_QuandoEncontrado()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        // Act
        var resultado = await _service.ObterPorIdAsync(alerta.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(alerta.Id);
        resultado.TalhaoId.Should().Be(alerta.TalhaoId);
        resultado.Tipo.Should().Be(alerta.Tipo);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id))
            .ReturnsAsync((Alerta?)null);

        // Act
        var resultado = await _service.ObterPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterTodosPorTalhaoAsync_DeveRetornarAlertasDoTalhao()
    {
        // Arrange
        var talhaoId = Guid.NewGuid();
        var alertas = new List<Alerta>
        {
            CriarAlertaValido(talhaoId),
            CriarAlertaValido(talhaoId)
        };

        _repositoryMock
            .Setup(x => x.ObterTodosPorTalhaoAsync(talhaoId))
            .ReturnsAsync(alertas);

        // Act
        var resultado = await _service.ObterTodosPorTalhaoAsync(talhaoId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(a => a.TalhaoId.Should().Be(talhaoId));
    }

    [Fact]
    public async Task ObterAtivosAsync_DeveRetornarApenasAlertasAtivos()
    {
        // Arrange
        var alertas = new List<Alerta>
        {
            CriarAlertaValido(status: StatusAlerta.Ativo),
            CriarAlertaValido(status: StatusAlerta.Ativo)
        };

        _repositoryMock
            .Setup(x => x.ObterAtivosAsync())
            .ReturnsAsync(alertas);

        // Act
        var resultado = await _service.ObterAtivosAsync();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(a => a.Status.Should().Be(StatusAlerta.Ativo));
    }

    [Fact]
    public async Task CriarAsync_DeveCriarAlertaComSucesso()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Seca,
            Severidade: NivelSeveridade.Alto,
            Titulo: "Alerta de Seca",
            Mensagem: "Umidade baixa detectada",
            Recomendacao: "Irrigação imediata",
            ValorReferencia: 25.5m
        );

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync((Alerta a) => a);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.TalhaoId.Should().Be(dto.TalhaoId);
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.Severidade.Should().Be(dto.Severidade);
        resultado.Status.Should().Be(StatusAlerta.Ativo);
        resultado.Titulo.Should().Be(dto.Titulo);
        resultado.Mensagem.Should().Be(dto.Mensagem);
        resultado.Recomendacao.Should().Be(dto.Recomendacao);
        resultado.ValorReferencia.Should().Be(dto.ValorReferencia);
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Alerta>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveAtualizarStatus()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        var novoStatus = StatusAlerta.EmAndamento;
        var dto = new AtualizarStatusAlertaDto(novoStatus);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AtualizarStatusAsync(alerta.Id, dto);

        // Assert
        alerta.Status.Should().Be(novoStatus);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Alerta>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveLancarExcecao_QuandoAlertaNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new AtualizarStatusAlertaDto(StatusAlerta.Visualizado);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id))
            .ReturnsAsync((Alerta?)null);

        // Act
        var act = async () => await _service.AtualizarStatusAsync(id, dto);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveSetarDataVisualizacao_QuandoStatusVisualizado()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        var dto = new AtualizarStatusAlertaDto(StatusAlerta.Visualizado);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AtualizarStatusAsync(alerta.Id, dto);

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Visualizado);
        alerta.DataVisualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task AtualizarStatusAsync_DeveSetarDataResolucao_QuandoStatusResolvido()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        var dto = new AtualizarStatusAlertaDto(StatusAlerta.Resolvido);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AtualizarStatusAsync(alerta.Id, dto);

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Resolvido);
        alerta.DataResolucao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task MarcarComoVisualizadoAsync_DeveMarcarComoVisualizado()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoVisualizadoAsync(alerta.Id);

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Visualizado);
        alerta.DataVisualizacao.Should().NotBeNull();
    }

    [Fact]
    public async Task MarcarComoResolvidoAsync_DeveMarcarComoResolvido()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(alerta.Id))
            .ReturnsAsync(alerta);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Alerta>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoResolvidoAsync(alerta.Id);

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Resolvido);
        alerta.DataResolucao.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterEstatisticasAsync_DeveRetornarEstatisticasCorretas()
    {
        // Arrange
        var alertas = new List<Alerta>
        {
            CriarAlertaValido(tipo: TipoAlerta.Seca, status: StatusAlerta.Ativo, severidade: NivelSeveridade.Alto),
            CriarAlertaValido(tipo: TipoAlerta.Geada, status: StatusAlerta.Visualizado, severidade: NivelSeveridade.Critico),
            CriarAlertaValido(tipo: TipoAlerta.Seca, status: StatusAlerta.Resolvido, severidade: NivelSeveridade.Medio),
            CriarAlertaValido(tipo: TipoAlerta.CalorExcessivo, status: StatusAlerta.Ativo, severidade: NivelSeveridade.Alto)
        };

        _repositoryMock
            .Setup(x => x.ObterAtivosAsync())
            .ReturnsAsync(alertas);

        // Act
        var resultado = await _service.ObterEstatisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalAlertas.Should().Be(4);
        resultado.AlertasAtivos.Should().Be(2);
        resultado.AlertasVisualizados.Should().Be(1);
        resultado.AlertasResolvidos.Should().Be(1);
        resultado.AlertasPorTipo.Should().ContainKey("Seca");
        resultado.AlertasPorTipo["Seca"].Should().Be(2);
        resultado.AlertasPorSeveridade.Should().ContainKey("Alto");
    }

    private static Alerta CriarAlertaValido(
        Guid? talhaoId = null,
        TipoAlerta tipo = TipoAlerta.Seca,
        StatusAlerta status = StatusAlerta.Ativo,
        NivelSeveridade severidade = NivelSeveridade.Medio)
    {
        return new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = talhaoId ?? Guid.NewGuid(),
            Tipo = tipo,
            Severidade = severidade,
            Status = status,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem de teste",
            Recomendacao = "Recomendação de teste",
            DataGeracao = DateTime.UtcNow,
            ValorReferencia = 25.5m
        };
    }
}
