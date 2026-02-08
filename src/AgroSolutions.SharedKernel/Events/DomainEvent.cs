namespace AgroSolutions.SharedKernel.Events;

/// <summary>
/// Evento de domínio base
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Data/hora em que o evento ocorreu (UTC)
    /// </summary>
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Tipo do evento
    /// </summary>
    public string EventType => GetType().Name;
}
