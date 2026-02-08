using AgroSolutions.Notificacoes.Domain.Enums;

namespace AgroSolutions.Notificacoes.Application.DTOs;

public record NotificacaoDto(
    Guid Id,
    Guid AlertaId,
    Guid TalhaoId,
    Guid DestinatarioId,
    string EmailDestinatario,
    string NomeDestinatario,
    string Tipo,
    string Status,
    string Prioridade,
    string Assunto,
    string Mensagem,
    DateTime DataCriacao,
    DateTime? DataEnvio,
    int TentativasEnvio
);

public record CriarNotificacaoDto(
    Guid AlertaId,
    Guid TalhaoId,
    Guid DestinatarioId,
    string EmailDestinatario,
    string NomeDestinatario,
    TipoNotificacao Tipo,
    PrioridadeNotificacao Prioridade,
    string Assunto,
    string Mensagem
);

public record EstatisticasNotificacoesDto(
    int TotalEnviadas,
    int TotalPendentes,
    int TotalFalhas,
    int EnviadasHoje,
    Dictionary<string, int> PorTipo
);
