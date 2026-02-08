using AgroSolutions.Analise.Domain.Enums;

namespace AgroSolutions.Analise.Application.Events;

/// <summary>
/// Evento publicado quando um alerta é gerado pelo motor de regras
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
