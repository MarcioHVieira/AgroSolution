using AgroSolutions.Notificacoes.Domain.Enums;

namespace AgroSolutions.Notificacoes.Application.Events;

/// <summary>
/// Evento publicado quando uma notificação é enviada com sucesso
/// </summary>
public record NotificacaoEnviadaEvent(
    Guid NotificacaoId,
    Guid AlertaId,
    Guid TalhaoId,
    Guid DestinatarioId,
    string EmailDestinatario,
    TipoNotificacao Tipo,
    DateTime DataEnvio,
    bool Sucesso,
    string? MensagemErro
);
