using AgroSolutions.Analise.Application.DTOs;
using AgroSolutions.Analise.Application.Events;
using AgroSolutions.Analise.Application.Interfaces;
using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Services;
using AgroSolutions.Analise.Infrastructure.Data;
using AgroSolutions.Analise.Infrastructure.Metrics;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Analise.Application.Services;

public class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _repository;
    private readonly ILogger<AlertaService> _logger;
    private readonly IRabbitMQAlertaPublisherService? _publisher;
    private readonly AnaliseDbContext _context;

    public AlertaService(
        IAlertaRepository repository,
        ILogger<AlertaService> logger,
        AnaliseDbContext context,
        IRabbitMQAlertaPublisherService? publisher = null)
    {
        _repository = repository;
        _logger = logger;
        _context = context;
        _publisher = publisher;
    }

    public async Task<AlertaDto?> ObterPorIdAsync(Guid id)
    {
        var alerta = await _repository.ObterPorIdAsync(id);
        return alerta == null ? null : MapearParaDto(alerta);
    }

    public async Task<IEnumerable<AlertaDto>> ObterTodosPorTalhaoAsync(Guid talhaoId)
    {
        var alertas = await _repository.ObterTodosPorTalhaoAsync(talhaoId);
        return alertas.Select(MapearParaDto);
    }

    public async Task<IEnumerable<AlertaDto>> ObterAtivosAsync()
    {
        var alertas = await _repository.ObterAtivosAsync();
        return alertas.Select(MapearParaDto);
    }

    public async Task<AlertaDto> CriarAsync(CriarAlertaDto dto)
    {
        var alerta = new Alerta
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

        var alertaCriado = await _repository.AdicionarAsync(alerta);
        _logger.LogInformation("Alerta criado: {AlertaId} - Tipo: {Tipo} - Talhão: {TalhaoId}",
            alertaCriado.Id, alertaCriado.Tipo, alertaCriado.TalhaoId);

        // Atualizar métricas Prometheus
        AtualizarMetricasAlerta(alertaCriado, ativo: true);

        await PublicarAlertaNoRabbitMQAsync(alertaCriado);

        return MapearParaDto(alertaCriado);
    }

    /// <summary>
    /// Publica alerta no RabbitMQ para notificar outros serviços em tempo real
    /// </summary>
    private async Task PublicarAlertaNoRabbitMQAsync(Alerta alerta)
    {
        if (_publisher == null)
        {
            _logger.LogDebug("RabbitMQ Publisher não configurado - Alerta não será publicado");
            return;
        }

        try
        {
            _logger.LogInformation("Buscando proprietário do talhão {TalhaoId}...", alerta.TalhaoId);
            var (destinatarioId, emailDestinatario, nomeDestinatario) = await ObterProprietarioTalhaoAsync(alerta.TalhaoId);

            if (string.IsNullOrEmpty(emailDestinatario))
            {
                _logger.LogWarning("Proprietário do talhão {TalhaoId} não encontrado. Evento será publicado SEM destinatário.", alerta.TalhaoId);
            }
            else
            {
                _logger.LogInformation("Proprietário encontrado: {Email} ({Nome})", emailDestinatario, nomeDestinatario);
            }

            var alertaEvento = new AlertaGeradoEvent(
                AlertaId: alerta.Id,
                TalhaoId: alerta.TalhaoId,
                Tipo: alerta.Tipo,
                Severidade: alerta.Severidade,
                Titulo: alerta.Titulo,
                Mensagem: alerta.Mensagem,
                Recomendacao: alerta.Recomendacao,
                DataGeracao: alerta.DataGeracao,
                ValorReferencia: alerta.ValorReferencia,
                DestinatarioId: destinatarioId,
                EmailDestinatario: emailDestinatario,
                NomeDestinatario: nomeDestinatario
            );

            _logger.LogInformation("Publicando evento com destinatário: Email={Email}, Nome={Nome}", 
                alertaEvento.EmailDestinatario ?? "NULL", 
                alertaEvento.NomeDestinatario ?? "NULL");

            // Routing key: alerta.{severidade}.{tipo}
            var routingKey = $"alerta.{alerta.Severidade.ToString().ToLower()}.{alerta.Tipo.ToString().ToLower()}";

            // Publicar com prioridade baseada na severidade
            bool sucesso = alerta.Severidade switch
            {
                NivelSeveridade.Critico => await _publisher.PublicarAlertaCriticoAsync(alertaEvento, routingKey),
                NivelSeveridade.Alto => await _publisher.PublicarAlertaAsync(alertaEvento, routingKey, prioridade: 8, ttlMinutos: 60),
                NivelSeveridade.Medio => await _publisher.PublicarAlertaNormalAsync(alertaEvento, routingKey),
                _ => await _publisher.PublicarAlertaAsync(alertaEvento, routingKey, prioridade: 3, ttlMinutos: 180)
            };

            if (sucesso)
            {
                _logger.LogInformation(
                    "Alerta publicado no RabbitMQ - ID: {AlertaId}, Tipo: {Tipo}, Severidade: {Severidade}, Routing: {RoutingKey}",
                    alerta.Id, alerta.Tipo, alerta.Severidade, routingKey);
            }
            else
            {
                _logger.LogWarning(
                    "Falha ao publicar alerta no RabbitMQ: {AlertaId}. Alerta salvo no banco mas não notificado.",
                    alerta.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao publicar alerta no RabbitMQ: {AlertaId}. Alerta salvo no banco mas notificação falhou.",
                alerta.Id);
        }
    }

    public async Task AtualizarStatusAsync(Guid id, AtualizarStatusAlertaDto dto)
    {
        var alerta = await _repository.ObterPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Alerta {id} não encontrado");

        var statusAnterior = alerta.Status;
        alerta.Status = dto.NovoStatus;

        if (dto.NovoStatus == StatusAlerta.Visualizado && !alerta.DataVisualizacao.HasValue)
        {
            alerta.DataVisualizacao = DateTime.UtcNow;
        }

        if (dto.NovoStatus == StatusAlerta.Resolvido && !alerta.DataResolucao.HasValue)
        {
            alerta.DataResolucao = DateTime.UtcNow;
            
            // Atualizar métricas - marcar alerta como inativo
            AtualizarMetricasAlerta(alerta, ativo: false);
        }

        await _repository.AtualizarAsync(alerta);
        _logger.LogInformation("Status do alerta {AlertaId} atualizado de {StatusAnterior} para {Status}", 
            id, statusAnterior, dto.NovoStatus);
    }

    public async Task MarcarComoVisualizadoAsync(Guid id)
    {
        await AtualizarStatusAsync(id, new AtualizarStatusAlertaDto(StatusAlerta.Visualizado));
    }

    public async Task MarcarComoResolvidoAsync(Guid id)
    {
        await AtualizarStatusAsync(id, new AtualizarStatusAlertaDto(StatusAlerta.Resolvido));
    }

    public async Task<EstatisticasAlertasDto> ObterEstatisticasAsync()
    {
        var todos = await _repository.ObterAtivosAsync();
        var alertasList = todos.ToList();

        var porTipo = alertasList
            .GroupBy(a => a.Tipo.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var porSeveridade = alertasList
            .GroupBy(a => a.Severidade.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        return new EstatisticasAlertasDto(
            TotalAlertas: alertasList.Count,
            AlertasAtivos: alertasList.Count(a => a.Status == StatusAlerta.Ativo),
            AlertasVisualizados: alertasList.Count(a => a.Status == StatusAlerta.Visualizado),
            AlertasResolvidos: alertasList.Count(a => a.Status == StatusAlerta.Resolvido),
            AlertasPorTipo: porTipo,
            AlertasPorSeveridade: porSeveridade
        );
    }

    private static AlertaDto MapearParaDto(Alerta alerta)
    {
        return new AlertaDto(
            Id: alerta.Id,
            TalhaoId: alerta.TalhaoId,
            Tipo: alerta.Tipo,
            TipoNome: alerta.Tipo.ToString(),
            Severidade: alerta.Severidade,
            SeveridadeNome: alerta.Severidade.ToString(),
            Status: alerta.Status,
            StatusNome: alerta.Status.ToString(),
            Titulo: alerta.Titulo,
            Mensagem: alerta.Mensagem,
            Recomendacao: alerta.Recomendacao,
            DataGeracao: alerta.DataGeracao,
            DataVisualizacao: alerta.DataVisualizacao,
            DataResolucao: alerta.DataResolucao,
            ValorReferencia: alerta.ValorReferencia
        );
    }

    /// <summary>
    /// Busca proprietário do talhão para incluir no evento
    /// </summary>
    private async Task<(Guid? UsuarioId, string? Email, string? Nome)> ObterProprietarioTalhaoAsync(Guid talhaoId)
    {
        try
        {
            var talhaoInfo = await _context.TalhoesInfo
                .Where(t => t.Id == talhaoId)
                .FirstOrDefaultAsync();

            if (talhaoInfo != null)
            {
                _logger.LogDebug("Proprietário do talhão {TalhaoId} encontrado no Read Model local: {Email}", 
                    talhaoId, talhaoInfo.EmailProprietario);
                return (talhaoInfo.ProprietarioId, talhaoInfo.EmailProprietario, talhaoInfo.NomeProprietario);
            }

            _logger.LogWarning("Proprietário do talhão {TalhaoId} não encontrado no Read Model local. ", talhaoId);
            return (null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar proprietário do talhão {TalhaoId} no Read Model", talhaoId);
            return (null, null, null);
        }
    }

    /// <summary>
    /// Atualiza métricas Prometheus para o alerta
    /// </summary>
    private void AtualizarMetricasAlerta(Alerta alerta, bool ativo)
    {
        try
        {
            var talhaoId = alerta.TalhaoId.ToString();
            //TODO: Substituir por nome real do talhão quando disponível, atualmente usando ID
            var talhaoNome = alerta.TalhaoId.ToString().Substring(0, 8); // Padrão: primeiros 8 chars
            var tipoAlerta = alerta.Tipo.ToString();
            var severidade = alerta.Severidade.ToString();

            // Buscar informações do talhão no Read Model (síncrono para não bloquear)
            var talhaoInfo = _context.TalhoesInfo
                .Where(t => t.Id == alerta.TalhaoId)
                .FirstOrDefault();

            if (talhaoInfo != null)
            {
                talhaoNome = talhaoInfo.Nome; // Nome real do talhão
            }

            // Atualizar alerta ativo/inativo
            AnaliseMetrics.AtualizarAlertaAtivo(tipoAlerta, talhaoNome, talhaoId, ativo);

            // Incrementar contador de alertas gerados (apenas quando ativo)
            if (ativo)
            {
                AnaliseMetrics.IncrementarAlertaGerado(tipoAlerta, severidade);
            }

            _logger.LogDebug("Métricas atualizadas: Tipo={Tipo}, Ativo={Ativo}, Talhao={Talhao}", 
                tipoAlerta, ativo, talhaoNome);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar métricas do alerta");
            // Não falha a operação se houver erro nas métricas
        }
    }
}

