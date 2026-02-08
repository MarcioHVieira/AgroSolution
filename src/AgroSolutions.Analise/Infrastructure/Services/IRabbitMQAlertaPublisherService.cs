namespace AgroSolutions.Analise.Infrastructure.Services;

/// <summary>
/// Interface para publicação de alertas no RabbitMQ
/// </summary>
public interface IRabbitMQAlertaPublisherService : IDisposable
{
    /// <summary>
    /// Publica um alerta crítico (prioridade 10, TTL 30min)
    /// </summary>
    Task<bool> PublicarAlertaCriticoAsync<T>(T alerta, string routingKey) where T : class;

    /// <summary>
    /// Publica um alerta normal (prioridade 5, TTL 120min)
    /// </summary>
    Task<bool> PublicarAlertaNormalAsync<T>(T alerta, string routingKey) where T : class;

    /// <summary>
    /// Publica um alerta com prioridade e TTL customizados
    /// </summary>
    Task<bool> PublicarAlertaAsync<T>(T alerta, string routingKey, int prioridade = 5, int ttlMinutos = 60) where T : class;
}
