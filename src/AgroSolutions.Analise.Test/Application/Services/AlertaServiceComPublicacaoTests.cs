using AgroSolutions.Analise.Application.DTOs;
using AgroSolutions.Analise.Application.Services;
using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Services;
using AgroSolutions.Analise.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Analise.Test.Application.Services;

public class AlertaServiceComPublicacaoTests
{
    private readonly Mock<IAlertaRepository> _repositoryMock;
    private readonly Mock<ILogger<AlertaService>> _loggerMock;
    private readonly AnaliseDbContext _context;
    private readonly Mock<IRabbitMQAlertaPublisherService> _publisherMock;
    private readonly AlertaService _service;

    public AlertaServiceComPublicacaoTests()
    {
        _repositoryMock = new Mock<IAlertaRepository>();
        _loggerMock = new Mock<ILogger<AlertaService>>();
        
        var options = new DbContextOptionsBuilder<AnaliseDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabaseComPublicacao")
            .Options;
        _context = new AnaliseDbContext(options);
        
        _publisherMock = new Mock<IRabbitMQAlertaPublisherService>();

        _service = new AlertaService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _context,
            _publisherMock.Object);
    }

    [Fact]
    public async Task CriarAsync_DevePublicarAlertaCritico_QuandoSeveridadeCritica()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Risco de Geada",
            Mensagem: "Temperatura -2°C",
            Recomendacao: "Ativar anti-geada",
            ValorReferencia: -2.0m
        );

        var alertaCriado = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = dto.TalhaoId,
            Tipo = dto.Tipo,
            Severidade = dto.Severidade,
            Status = StatusAlerta.Ativo,
            Titulo = dto.Titulo,
            Mensagem = dto.Mensagem,
            Recomendacao = dto.Recomendacao,
            ValorReferencia = dto.ValorReferencia,
            DataGeracao = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Severidade.Should().Be(NivelSeveridade.Critico);

        _publisherMock.Verify(
            x => x.PublicarAlertaCriticoAsync(
                It.Is<object>(a => a.ToString()!.Contains("Geada")),
                It.Is<string>(r => r.Contains("critico") && r.Contains("geada"))),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DevePublicarAlertaAlto_QuandoSeveridadeAlta()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Seca,
            Severidade: NivelSeveridade.Alto,
            Titulo: "Alerta de Seca",
            Mensagem: "Umidade 25%",
            Recomendacao: "Irrigação urgente",
            ValorReferencia: 25m
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaAsync(
                It.IsAny<object>(), 
                It.IsAny<string>(), 
                It.IsAny<int>(), 
                It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();

        _publisherMock.Verify(
            x => x.PublicarAlertaAsync(
                It.IsAny<object>(),
                It.Is<string>(r => r.Contains("alto") && r.Contains("seca")),
                8, // Prioridade alta
                60), // TTL 60 minutos
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DevePublicarAlertaNormal_QuandoSeveridadeMedia()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.ExcessoUmidade,
            Severidade: NivelSeveridade.Medio,
            Titulo: "Excesso de Umidade",
            Mensagem: "Umidade 90%",
            Recomendacao: "Verificar drenagem",
            ValorReferencia: 90m
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaNormalAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();

        _publisherMock.Verify(
            x => x.PublicarAlertaNormalAsync(
                It.IsAny<object>(),
                It.Is<string>(r => r.Contains("medio") && r.Contains("excessoumidade"))),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveSalvarAlerta_MesmoSePublicacaoFalhar()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Teste",
            Mensagem: "Teste",
            Recomendacao: "Teste",
            ValorReferencia: null
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        // Simular falha na publicação
        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("RabbitMQ não disponível"));

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(alertaCriado.Id);

        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Alerta>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLogarErro_QuandoPublicacaoFalhar()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Teste",
            Mensagem: "Teste",
            Recomendacao: null,
            ValorReferencia: null
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Conexão falhou"));

        // Act
        await _service.CriarAsync(dto);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao publicar")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_NaoDevePublicar_QuandoPublisherNull()
    {
        // Arrange
        var serviceSePublisher = new AlertaService(
            _repositoryMock.Object,
            _loggerMock.Object,
            _context,
            publisher: null); // Sem publisher

        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Teste",
            Mensagem: "Teste",
            Recomendacao: null,
            ValorReferencia: null
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        // Act
        var resultado = await serviceSePublisher.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();

        // Deve logar que publisher não está configurado
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("não configurado")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(TipoAlerta.Seca, "seca")]
    [InlineData(TipoAlerta.Geada, "geada")]
    [InlineData(TipoAlerta.CalorExcessivo, "calorexcessivo")]
    [InlineData(TipoAlerta.ExcessoUmidade, "excessoumidade")]
    [InlineData(TipoAlerta.RiscoPraga, "riscopraga")]
    public async Task CriarAsync_DeveGerarRoutingKeyCorreta_ParaCadaTipoDeAlerta(
        TipoAlerta tipo, 
        string tipoEsperado)
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: tipo,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Teste",
            Mensagem: "Teste",
            Recomendacao: null,
            ValorReferencia: null
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.CriarAsync(dto);

        // Assert
        _publisherMock.Verify(
            x => x.PublicarAlertaCriticoAsync(
                It.IsAny<object>(),
                It.Is<string>(r => r.Contains(tipoEsperado))),
            Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveIncluirTodosDadosNoPayload()
    {
        // Arrange
        var talhaoId = Guid.NewGuid();
        var dto = new CriarAlertaDto(
            TalhaoId: talhaoId,
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Risco de Geada",
            Mensagem: "Temperatura -2°C",
            Recomendacao: "Ativar sistema",
            ValorReferencia: -2.0m
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        object? payloadCapturado = null;
        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .Callback<object, string>((payload, routing) => payloadCapturado = payload)
            .ReturnsAsync(true);

        // Act
        await _service.CriarAsync(dto);

        // Assert
        payloadCapturado.Should().NotBeNull();
        var payloadStr = payloadCapturado!.ToString();
        payloadStr.Should().Contain(talhaoId.ToString());
        payloadStr.Should().Contain("Geada");
        payloadStr.Should().Contain("Critico");
    }

    [Fact]
    public async Task CriarAsync_DeveLogarSucesso_QuandoPublicarComSucesso()
    {
        // Arrange
        var dto = new CriarAlertaDto(
            TalhaoId: Guid.NewGuid(),
            Tipo: TipoAlerta.Geada,
            Severidade: NivelSeveridade.Critico,
            Titulo: "Teste",
            Mensagem: "Teste",
            Recomendacao: null,
            ValorReferencia: null
        );

        var alertaCriado = CriarAlertaValido(dto);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Alerta>()))
            .ReturnsAsync(alertaCriado);

        _publisherMock
            .Setup(x => x.PublicarAlertaCriticoAsync(It.IsAny<object>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await _service.CriarAsync(dto);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("publicado no RabbitMQ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static Alerta CriarAlertaValido(CriarAlertaDto dto)
    {
        return new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = dto.TalhaoId,
            Tipo = dto.Tipo,
            Severidade = dto.Severidade,
            Status = StatusAlerta.Ativo,
            Titulo = dto.Titulo,
            Mensagem = dto.Mensagem,
            Recomendacao = dto.Recomendacao,
            ValorReferencia = dto.ValorReferencia,
            DataGeracao = DateTime.UtcNow
        };
    }
}
