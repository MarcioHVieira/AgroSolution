namespace AgroSolutions.Propriedades.Application.Interfaces;

/// <summary>
/// Interface para publicação de eventos no message bus (RabbitMQ)
/// </summary>
public interface IMessageBusPublisher
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
}
