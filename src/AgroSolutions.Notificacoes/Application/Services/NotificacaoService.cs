using AgroSolutions.Notificacoes.Application.DTOs;
using AgroSolutions.Notificacoes.Application.Events;
using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;

namespace AgroSolutions.Notificacoes.Application.Services;

public class NotificacaoService : INotificacaoService
{
    private readonly INotificacaoRepository _repository;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<NotificacaoService> _logger;

    public NotificacaoService(
        INotificacaoRepository repository, 
        IRabbitMQPublisher publisher,
        ILogger<NotificacaoService> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<NotificacaoDto?> ObterPorIdAsync(Guid id)
    {
        var notificacao = await _repository.ObterPorIdAsync(id);
        return notificacao == null ? null : MapearParaDto(notificacao);
    }

    public async Task<IEnumerable<NotificacaoDto>> ObterTodasAsync()
    {
        var notificacoes = await _repository.ObterTodasAsync();
        return notificacoes.Select(MapearParaDto);
    }

    public async Task<IEnumerable<NotificacaoDto>> ObterPorDestinatarioAsync(Guid destinatarioId)
    {
        var notificacoes = await _repository.ObterPorDestinatarioAsync(destinatarioId);
        return notificacoes.Select(MapearParaDto);
    }

    public async Task<NotificacaoDto> CriarAsync(CriarNotificacaoDto dto)
    {
        var notificacao = new Notificacao
        {
            Id = Guid.NewGuid(),
            AlertaId = dto.AlertaId,
            TalhaoId = dto.TalhaoId,
            DestinatarioId = dto.DestinatarioId,
            EmailDestinatario = dto.EmailDestinatario,
            NomeDestinatario = dto.NomeDestinatario,
            Tipo = dto.Tipo,
            Status = StatusNotificacao.Pendente,
            Prioridade = dto.Prioridade,
            Assunto = dto.Assunto,
            Mensagem = dto.Mensagem,
            DataCriacao = DateTime.UtcNow,
            TentativasEnvio = 0
        };

        var criada = await _repository.AdicionarAsync(notificacao);
        _logger.LogInformation("Notificação criada: {NotificacaoId} para {Email}", criada.Id, criada.EmailDestinatario);
        
        return MapearParaDto(criada);
    }

    public async Task<EstatisticasNotificacoesDto> ObterEstatisticasAsync()
    {
        var todas = await _repository.ObterTodasAsync();
        var lista = todas.ToList();
        var hoje = DateTime.UtcNow.Date;

        return new EstatisticasNotificacoesDto(
            TotalEnviadas: lista.Count(n => n.Status == StatusNotificacao.Enviada),
            TotalPendentes: lista.Count(n => n.Status == StatusNotificacao.Pendente),
            TotalFalhas: lista.Count(n => n.Status == StatusNotificacao.Falha),
            EnviadasHoje: lista.Count(n => n.DataEnvio?.Date == hoje),
            PorTipo: lista.GroupBy(n => n.Tipo.ToString()).ToDictionary(g => g.Key, g => g.Count())
        );
    }

    public async Task MarcarComoEnviadaAsync(Guid notificacaoId, bool sucesso, string? mensagemErro = null)
    {
        var notificacao = await _repository.ObterPorIdAsync(notificacaoId);
        if (notificacao == null)
        {
            _logger.LogWarning("Notificação {NotificacaoId} não encontrada", notificacaoId);
            return;
        }

        if (sucesso)
        {
            notificacao.Status = StatusNotificacao.Enviada;
            notificacao.DataEnvio = DateTime.UtcNow;
            _logger.LogInformation("Notificação {NotificacaoId} enviada com sucesso", notificacaoId);
        }
        else
        {
            notificacao.Status = StatusNotificacao.Falha;
            notificacao.MensagemErro = mensagemErro;
            _logger.LogError("Falha ao enviar notificação {NotificacaoId}: {Erro}", notificacaoId, mensagemErro);
        }

        await _repository.AtualizarAsync(notificacao);

        // Publicar evento NotificacaoEnviada
        var routingKey = sucesso 
            ? $"notificacao.enviada.{notificacao.Tipo.ToString().ToLower()}" 
            : $"notificacao.falha.{notificacao.Tipo.ToString().ToLower()}";
        
        await _publisher.PublishAsync(new NotificacaoEnviadaEvent(
            NotificacaoId: notificacao.Id,
            AlertaId: notificacao.AlertaId,
            TalhaoId: notificacao.TalhaoId,
            DestinatarioId: notificacao.DestinatarioId,
            EmailDestinatario: notificacao.EmailDestinatario,
            Tipo: notificacao.Tipo,
            DataEnvio: notificacao.DataEnvio ?? DateTime.UtcNow,
            Sucesso: sucesso,
            MensagemErro: mensagemErro
        ), routingKey);
    }

    private static NotificacaoDto MapearParaDto(Notificacao n) => new(
        n.Id, n.AlertaId, n.TalhaoId, n.DestinatarioId, n.EmailDestinatario, n.NomeDestinatario,
        n.Tipo.ToString(), n.Status.ToString(), n.Prioridade.ToString(),
        n.Assunto, n.Mensagem, n.DataCriacao, n.DataEnvio, n.TentativasEnvio
    );
}
