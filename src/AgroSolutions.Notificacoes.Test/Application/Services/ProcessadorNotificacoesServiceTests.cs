using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Application.Services;
using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Notificacoes.Test.Application.Services;

public class ProcessadorNotificacoesServiceTests
{
    private readonly Mock<INotificacaoRepository> _repositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<INotificacaoService> _notificacaoServiceMock;
    private readonly Mock<ILogger<ProcessadorNotificacoesService>> _loggerMock;
    private readonly ProcessadorNotificacoesService _service;

    public ProcessadorNotificacoesServiceTests()
    {
        _repositoryMock = new Mock<INotificacaoRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _notificacaoServiceMock = new Mock<INotificacaoService>();
        _loggerMock = new Mock<ILogger<ProcessadorNotificacoesService>>();
        _service = new ProcessadorNotificacoesService(
            _repositoryMock.Object,
            _emailServiceMock.Object,
            _notificacaoServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveProcessarNotificacoesComSucesso()
    {
        // Arrange
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoPendente()
        };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        _notificacaoServiceMock
            .Setup(x => x.MarcarComoEnviadaAsync(It.IsAny<Guid>(), true, null))
            .Callback<Guid, bool, string?>((id, sucesso, erro) =>
            {
                var notificacao = notificacoes.First(n => n.Id == id);
                notificacao.Status = StatusNotificacao.Enviada;
                notificacao.DataEnvio = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacoes[0].Status.Should().Be(StatusNotificacao.Enviada);
        notificacoes[0].DataEnvio.Should().NotBeNull();
        _emailServiceMock.Verify(
            x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
        _notificacaoServiceMock.Verify(x => x.MarcarComoEnviadaAsync(It.IsAny<Guid>(), true, null), Times.Once);
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveMarcarComoReenviando_QuandoFalhaMenosDe3Vezes()
    {
        // Arrange
        var notificacao = CriarNotificacaoPendente();
        notificacao.TentativasEnvio = 1;

        var notificacoes = new List<Notificacao> { notificacao };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacao.Status.Should().Be(StatusNotificacao.Reenviando);
        notificacao.TentativasEnvio.Should().Be(2);
        notificacao.DataEnvio.Should().BeNull();
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveMarcarComoFalha_QuandoAtinge3Tentativas()
    {
        // Arrange
        var notificacao = CriarNotificacaoPendente();
        notificacao.TentativasEnvio = 2;

        var notificacoes = new List<Notificacao> { notificacao };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacao.Status.Should().Be(StatusNotificacao.Falha);
        notificacao.TentativasEnvio.Should().Be(3);
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveProcessarMultiplasNotificacoes()
    {
        // Arrange
        var notificacoes = new List<Notificacao>
        {
            CriarNotificacaoPendente(),
            CriarNotificacaoPendente(),
            CriarNotificacaoPendente()
        };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        _notificacaoServiceMock
            .Setup(x => x.MarcarComoEnviadaAsync(It.IsAny<Guid>(), true, null))
            .Callback<Guid, bool, string?>((id, sucesso, erro) =>
            {
                var notificacao = notificacoes.First(n => n.Id == id);
                notificacao.Status = StatusNotificacao.Enviada;
                notificacao.DataEnvio = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacoes.Should().AllSatisfy(n =>
        {
            n.Status.Should().Be(StatusNotificacao.Enviada);
            n.DataEnvio.Should().NotBeNull();
        });
        _emailServiceMock.Verify(
            x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Exactly(3));
        _notificacaoServiceMock.Verify(x => x.MarcarComoEnviadaAsync(It.IsAny<Guid>(), true, null), Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveTratarExcecao_EMarcarComoFalha()
    {
        // Arrange
        var notificacao = CriarNotificacaoPendente();
        var notificacoes = new List<Notificacao> { notificacao };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Erro no servidor SMTP"));

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacao.Status.Should().Be(StatusNotificacao.Falha);
        notificacao.TentativasEnvio.Should().Be(1);
        notificacao.MensagemErro.Should().Contain("SMTP");
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_NaoDeveProcessar_QuandoNaoHaPendentes()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(new List<Notificacao>());

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        _emailServiceMock.Verify(
            x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveProcessarApenasNotificacoesEmail()
    {
        // Arrange
        var notificacao = CriarNotificacaoPendente();
        notificacao.Tipo = TipoNotificacao.Email;

        var notificacoes = new List<Notificacao> { notificacao };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        _emailServiceMock.Verify(
            x => x.EnviarEmailAsync(notificacao.EmailDestinatario, notificacao.Assunto, notificacao.Mensagem),
            Times.Once);
    }

    [Fact]
    public async Task ProcessarNotificacoesPendentesAsync_DeveDefinirDataEnvio_QuandoSucesso()
    {
        // Arrange
        var notificacao = CriarNotificacaoPendente();
        var notificacoes = new List<Notificacao> { notificacao };

        _repositoryMock
            .Setup(x => x.ObterPendentesAsync())
            .ReturnsAsync(notificacoes);

        _emailServiceMock
            .Setup(x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Notificacao>()))
            .Returns(Task.CompletedTask);

        _notificacaoServiceMock
            .Setup(x => x.MarcarComoEnviadaAsync(It.IsAny<Guid>(), true, null))
            .Callback<Guid, bool, string?>((id, sucesso, erro) =>
            {
                var notif = notificacoes.First(n => n.Id == id);
                notif.Status = StatusNotificacao.Enviada;
                notif.DataEnvio = DateTime.UtcNow;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _service.ProcessarNotificacoesPendentesAsync();

        // Assert
        notificacao.DataEnvio.Should().NotBeNull();
        notificacao.DataEnvio.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    private static Notificacao CriarNotificacaoPendente()
    {
        return new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = Guid.NewGuid(),
            EmailDestinatario = "teste@email.com",
            NomeDestinatario = "Teste",
            Tipo = TipoNotificacao.Email,
            Status = StatusNotificacao.Pendente,
            Prioridade = PrioridadeNotificacao.Normal,
            Assunto = "Teste",
            Mensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };
    }
}
