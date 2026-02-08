using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Notificacoes.Test.Domain.Entities;

public class NotificacaoTests
{
    [Fact]
    public void Notificacao_DeveSerCriadaComPropriedadesValidas()
    {
        // Arrange & Act
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = Guid.NewGuid(),
            EmailDestinatario = "produtor@fazenda.com",
            NomeDestinatario = "Marcio Henrique",
            Tipo = TipoNotificacao.Email,
            Status = StatusNotificacao.Pendente,
            Prioridade = PrioridadeNotificacao.Alta,
            Assunto = "Alerta de Seca - Talhão Norte",
            Mensagem = "Atenção: Umidade do solo abaixo do limite crítico",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        // Assert
        notificacao.Id.Should().NotBeEmpty();
        notificacao.AlertaId.Should().NotBeEmpty();
        notificacao.TalhaoId.Should().NotBeEmpty();
        notificacao.DestinatarioId.Should().NotBeEmpty();
        notificacao.EmailDestinatario.Should().Be("produtor@fazenda.com");
        notificacao.NomeDestinatario.Should().Be("Marcio Henrique");
        notificacao.Tipo.Should().Be(TipoNotificacao.Email);
        notificacao.Status.Should().Be(StatusNotificacao.Pendente);
        notificacao.Prioridade.Should().Be(PrioridadeNotificacao.Alta);
        notificacao.Assunto.Should().NotBeEmpty();
        notificacao.Mensagem.Should().NotBeEmpty();
        notificacao.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        notificacao.TentativasEnvio.Should().Be(0);
        notificacao.DataEnvio.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoNotificacao.Email)]
    [InlineData(TipoNotificacao.SMS)]
    [InlineData(TipoNotificacao.Push)]
    [InlineData(TipoNotificacao.InApp)]
    public void Notificacao_DeveAceitarTodosTiposDeNotificacao(TipoNotificacao tipo)
    {
        // Arrange & Act
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = Guid.NewGuid(),
            EmailDestinatario = "teste@email.com",
            NomeDestinatario = "Teste",
            Tipo = tipo,
            Status = StatusNotificacao.Pendente,
            Prioridade = PrioridadeNotificacao.Normal,
            Assunto = "Teste",
            Mensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        // Assert
        notificacao.Tipo.Should().Be(tipo);
    }

    [Theory]
    [InlineData(StatusNotificacao.Pendente)]
    [InlineData(StatusNotificacao.Enviada)]
    [InlineData(StatusNotificacao.Falha)]
    [InlineData(StatusNotificacao.Reenviando)]
    public void Notificacao_DeveAceitarTodosStatusPossiveis(StatusNotificacao status)
    {
        // Arrange & Act
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = Guid.NewGuid(),
            EmailDestinatario = "teste@email.com",
            NomeDestinatario = "Teste",
            Tipo = TipoNotificacao.Email,
            Status = status,
            Prioridade = PrioridadeNotificacao.Normal,
            Assunto = "Teste",
            Mensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        // Assert
        notificacao.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(PrioridadeNotificacao.Baixa)]
    [InlineData(PrioridadeNotificacao.Normal)]
    [InlineData(PrioridadeNotificacao.Alta)]
    [InlineData(PrioridadeNotificacao.Urgente)]
    public void Notificacao_DeveAceitarTodasPrioridades(PrioridadeNotificacao prioridade)
    {
        // Arrange & Act
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            DestinatarioId = Guid.NewGuid(),
            EmailDestinatario = "teste@email.com",
            NomeDestinatario = "Teste",
            Tipo = TipoNotificacao.Email,
            Status = StatusNotificacao.Pendente,
            Prioridade = prioridade,
            Assunto = "Teste",
            Mensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        // Assert
        notificacao.Prioridade.Should().Be(prioridade);
    }

    [Fact]
    public void Notificacao_DevePermitirRegistrarDataEnvio()
    {
        // Arrange
        var notificacao = new Notificacao
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

        var dataEnvio = DateTime.UtcNow;

        // Act
        notificacao.Status = StatusNotificacao.Enviada;
        notificacao.DataEnvio = dataEnvio;

        // Assert
        notificacao.Status.Should().Be(StatusNotificacao.Enviada);
        notificacao.DataEnvio.Should().BeCloseTo(dataEnvio, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Notificacao_DevePermitirIncrementarTentativasEnvio()
    {
        // Arrange
        var notificacao = new Notificacao
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

        // Act
        notificacao.TentativasEnvio++;
        notificacao.TentativasEnvio++;
        notificacao.TentativasEnvio++;

        // Assert
        notificacao.TentativasEnvio.Should().Be(3);
    }

    [Fact]
    public void Notificacao_DevePermitirRegistrarMensagemErro()
    {
        // Arrange
        var notificacao = new Notificacao
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

        var mensagemErro = "Erro ao conectar com servidor SMTP";

        // Act
        notificacao.Status = StatusNotificacao.Falha;
        notificacao.MensagemErro = mensagemErro;

        // Assert
        notificacao.Status.Should().Be(StatusNotificacao.Falha);
        notificacao.MensagemErro.Should().Be(mensagemErro);
    }

    [Fact]
    public void Notificacao_DevePermitirDadosAdicionaisEmJSON()
    {
        // Arrange & Act
        var dadosJson = "{\"severidade\":\"Alta\",\"tipo_alerta\":\"Seca\",\"valor\":25.5}";
        var notificacao = new Notificacao
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
            TentativasEnvio = 0,
            DadosAdicionais = dadosJson
        };

        // Assert
        notificacao.DadosAdicionais.Should().Be(dadosJson);
        notificacao.DadosAdicionais.Should().Contain("severidade");
    }

    [Fact]
    public void Notificacao_DevePermitirValoresNulos()
    {
        // Arrange & Act
        var notificacao = new Notificacao
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
            TentativasEnvio = 0,
            DataEnvio = null,
            MensagemErro = null,
            DadosAdicionais = null
        };

        // Assert
        notificacao.DataEnvio.Should().BeNull();
        notificacao.MensagemErro.Should().BeNull();
        notificacao.DadosAdicionais.Should().BeNull();
    }
}
