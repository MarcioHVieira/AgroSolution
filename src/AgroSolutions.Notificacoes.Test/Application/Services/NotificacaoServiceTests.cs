using AgroSolutions.Notificacoes.Application.DTOs;
using AgroSolutions.Notificacoes.Application.Services;
using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Notificacoes.Test.Application.Services;

public class NotificacaoServiceTests
{
    private readonly Mock<INotificacaoRepository> _repositoryMock;
    private readonly Mock<IRabbitMQPublisher> _publisherMock;
    private readonly Mock<ILogger<NotificacaoService>> _loggerMock;
    private readonly NotificacaoService _service;

    public NotificacaoServiceTests()
    {
        _repositoryMock = new Mock<INotificacaoRepository>();
        _publisherMock = new Mock<IRabbitMQPublisher>();
        _loggerMock = new Mock<ILogger<NotificacaoService>>();
        _service = new NotificacaoService(_repositoryMock.Object, _publisherMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNotificacao_QuandoEncontrada()
    {
        // Arrange
        var notificacao = CriarNotificacaoValida();
        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(notificacao.Id))
            .ReturnsAsync(notificacao);

        // Act
        var resultado = await _service.ObterPorIdAsync(notificacao.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(notificacao.Id);
        resultado.EmailDestinatario.Should().Be(notificacao.EmailDestinatario);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNull_QuandoNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id))
            .ReturnsAsync((Notificacao?)null);

        // Act
        var resultado = await _service.ObterPorIdAsync(id);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarTodasNotificacoes()
    {
        // Arrange
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoValida(),
            CriarNotificacaoValida()
        };

        _repositoryMock
            .Setup(x => x.ObterTodasAsync())
            .ReturnsAsync(notificacoes);

        // Act
        var resultado = await _service.ObterTodasAsync();

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterPorDestinatarioAsync_DeveRetornarNotificacoesDoDestinatario()
    {
        // Arrange
        var destinatarioId = Guid.NewGuid();
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoValida(destinatarioId),
            CriarNotificacaoValida(destinatarioId)
        };

        _repositoryMock
            .Setup(x => x.ObterPorDestinatarioAsync(destinatarioId))
            .ReturnsAsync(notificacoes);

        // Act
        var resultado = await _service.ObterPorDestinatarioAsync(destinatarioId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(n => n.DestinatarioId.Should().Be(destinatarioId));
    }

    [Fact]
    public async Task CriarAsync_DeveCriarNotificacaoComStatusPendente()
    {
        // Arrange
        var dto = new CriarNotificacaoDto(
            AlertaId: Guid.NewGuid(),
            TalhaoId: Guid.NewGuid(),
            DestinatarioId: Guid.NewGuid(),
            EmailDestinatario: "produtor@fazenda.com",
            NomeDestinatario: "João Silva",
            Tipo: TipoNotificacao.Email,
            Prioridade: PrioridadeNotificacao.Alta,
            Assunto: "Alerta de Seca",
            Mensagem: "Umidade baixa detectada"
        );

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Notificacao>()))
            .ReturnsAsync((Notificacao n) => n);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.EmailDestinatario.Should().Be(dto.EmailDestinatario);
        resultado.Status.Should().Be(StatusNotificacao.Pendente.ToString());
        resultado.TentativasEnvio.Should().Be(0);
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Notificacao>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveDefinirDataCriacaoComoAgora()
    {
        // Arrange
        var dto = new CriarNotificacaoDto(
            AlertaId: Guid.NewGuid(),
            TalhaoId: Guid.NewGuid(),
            DestinatarioId: Guid.NewGuid(),
            EmailDestinatario: "teste@email.com",
            NomeDestinatario: "Teste",
            Tipo: TipoNotificacao.Email,
            Prioridade: PrioridadeNotificacao.Normal,
            Assunto: "Teste",
            Mensagem: "Mensagem teste"
        );

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Notificacao>()))
            .ReturnsAsync((Notificacao n) => n);

        // Act
        var resultado = await _service.CriarAsync(dto);

        // Assert
        resultado.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ObterEstatisticasAsync_DeveRetornarEstatisticasCorretas()
    {
        // Arrange
        var hoje = DateTime.UtcNow.Date;
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoValida(status: StatusNotificacao.Enviada, dataEnvio: hoje),
            CriarNotificacaoValida(status: StatusNotificacao.Enviada, dataEnvio: hoje.AddDays(-1)),
            CriarNotificacaoValida(status: StatusNotificacao.Pendente),
            CriarNotificacaoValida(status: StatusNotificacao.Pendente),
            CriarNotificacaoValida(status: StatusNotificacao.Falha)
        };

        _repositoryMock
            .Setup(x => x.ObterTodasAsync())
            .ReturnsAsync(notificacoes);

        // Act
        var resultado = await _service.ObterEstatisticasAsync();

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalEnviadas.Should().Be(2);
        resultado.TotalPendentes.Should().Be(2);
        resultado.TotalFalhas.Should().Be(1);
        resultado.EnviadasHoje.Should().Be(1);
        resultado.PorTipo.Should().ContainKey("Email");
    }

    [Fact]
    public async Task ObterEstatisticasAsync_DeveAgruparPorTipo()
    {
        // Arrange
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoValida(tipo: TipoNotificacao.Email),
            CriarNotificacaoValida(tipo: TipoNotificacao.Email),
            CriarNotificacaoValida(tipo: TipoNotificacao.SMS),
            CriarNotificacaoValida(tipo: TipoNotificacao.Push)
        };

        _repositoryMock
            .Setup(x => x.ObterTodasAsync())
            .ReturnsAsync(notificacoes);

        // Act
        var resultado = await _service.ObterEstatisticasAsync();

        // Assert
        resultado.PorTipo.Should().HaveCount(3);
        resultado.PorTipo["Email"].Should().Be(2);
        resultado.PorTipo["SMS"].Should().Be(1);
        resultado.PorTipo["Push"].Should().Be(1);
    }

    private static Notificacao CriarNotificacaoValida(
        Guid? destinatarioId = null,
        TipoNotificacao tipo = TipoNotificacao.Email,
        StatusNotificacao status = StatusNotificacao.Pendente,
        DateTime? dataEnvio = null)
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = destinatarioId ?? Guid.NewGuid(),
            EmailDestinatario = "teste@email.com",
            NomeDestinatario = "Teste",
            Tipo = tipo,
            Status = status,
            Prioridade = PrioridadeNotificacao.Normal,
            Assunto = "Teste",
            Mensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        if (dataEnvio.HasValue)
        {
            notificacao.DataEnvio = dataEnvio.Value;
        }

        return notificacao;
    }
}
