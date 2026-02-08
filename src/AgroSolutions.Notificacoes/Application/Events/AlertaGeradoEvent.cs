using AgroSolutions.Notificacoes.Domain.Enums;

namespace AgroSolutions.Notificacoes.Application.Events;

/// <summary>
/// Evento recebido quando um alerta é gerado pelo microserviço Analise
/// </summary>
public record AlertaGeradoEvent(
    Guid AlertaId,
    Guid TalhaoId,
    TipoAlerta Tipo,
    NivelSeveridade Severidade,
    string Titulo,
    string Mensagem,
    string? Recomendacao,
    DateTime DataGeracao,
    decimal? ValorReferencia,
    Guid? DestinatarioId,
    string? EmailDestinatario,
    string? NomeDestinatario
);
