namespace AgroSolutions.SharedKernel.Messaging;

/// <summary>
/// Interface genérica para publicação de eventos no RabbitMQ
/// </summary>
public interface IRabbitMQPublisher : IDisposable
{
    /// <summary>
    /// Publica um evento genérico no RabbitMQ
    /// </summary>
    /// <typeparam name="T">Tipo do evento</typeparam>
    /// <param name="evento">Evento a ser publicado</param>
    /// <param name="routingKey">Chave de roteamento (ex: "usuario.criado")</param>
    /// <param name="persistent">Se a mensagem deve ser persistida (padrão: true)</param>
    Task PublishAsync<T>(T evento, string routingKey, bool persistent = true) where T : class;
}
