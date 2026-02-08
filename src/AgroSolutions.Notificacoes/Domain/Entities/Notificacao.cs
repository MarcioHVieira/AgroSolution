using AgroSolutions.Notificacoes.Domain.Enums;

namespace AgroSolutions.Notificacoes.Domain.Entities;

/// <summary>
/// Entidade de Notificação
/// </summary>
public class Notificacao
{
    /// <summary>
    /// Identificador único da notificação
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID do alerta relacionado (origem da notificação)
    /// </summary>
    public Guid AlertaId { get; set; }

    /// <summary>
    /// ID do talhão relacionado
    /// </summary>
    public Guid TalhaoId { get; set; }

    /// <summary>
    /// ID do destinatário (usuário)
    /// </summary>
    public Guid DestinatarioId { get; set; }

    /// <summary>
    /// E-mail do destinatário
    /// </summary>
    public string EmailDestinatario { get; set; } = string.Empty;

    /// <summary>
    /// Nome do destinatário
    /// </summary>
    public string NomeDestinatario { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificação
    /// </summary>
    public TipoNotificacao Tipo { get; set; }

    /// <summary>
    /// Status da notificação
    /// </summary>
    public StatusNotificacao Status { get; set; }

    /// <summary>
    /// Prioridade da notificação
    /// </summary>
    public PrioridadeNotificacao Prioridade { get; set; }

    /// <summary>
    /// Assunto da notificação
    /// </summary>
    public string Assunto { get; set; } = string.Empty;

    /// <summary>
    /// Corpo da mensagem (HTML ou texto)
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora de criação
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data e hora do envio
    /// </summary>
    public DateTime? DataEnvio { get; set; }

    /// <summary>
    /// Número de tentativas de envio
    /// </summary>
    public int TentativasEnvio { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver falha)
    /// </summary>
    public string? MensagemErro { get; set; }

    /// <summary>
    /// Dados adicionais em JSON
    /// </summary>
    public string? DadosAdicionais { get; set; }
}
