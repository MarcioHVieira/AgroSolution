namespace AgroSolutions.SharedKernel.Events;

/// <summary>
/// Evento de integração base para comunicação entre microserviços
/// </summary>
public abstract record IntegrationEvent
{
    /// <summary>
    /// Identificador único do evento
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Data/hora em que o evento ocorreu (UTC)
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Tipo do evento (nome da classe)
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Versão do evento (para versionamento de schema)
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// ID de correlação para rastreamento distribuído
    /// </summary>
    public string? CorrelationId { get; init; }
}
