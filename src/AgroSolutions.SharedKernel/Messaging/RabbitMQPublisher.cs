using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AgroSolutions.SharedKernel.Messaging;

/// <summary>
/// Implementação genérica e robusta de publisher RabbitMQ para todos os microserviços
/// Inclui reconnection, retry logic e proper disposal
/// </summary>
public class RabbitMQPublisher : IRabbitMQPublisher
{
    private readonly ILogger<RabbitMQPublisher> _logger;
    private readonly RabbitMQSettings _settings;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;

    public RabbitMQPublisher(
        ILogger<RabbitMQPublisher> logger,
        IOptions<RabbitMQSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        
        // Inicializa a conexão de forma assíncrona
        _ = EnsureConnectionAsync();
    }

    /// <summary>
    /// Garante que a conexão está ativa, reconectando se necessário
    /// </summary>
    private async Task<IChannel> EnsureConnectionAsync()
    {
        if (_channel != null && _channel.IsOpen)
            return _channel;

        await _connectionLock.WaitAsync();
        try
        {
            // Double-check após obter o lock
            if (_channel != null && _channel.IsOpen)
                return _channel;

            // Fecha conexões anteriores se existirem
            await CloseConnectionAsync();

            // Cria nova conexão
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_settings.NetworkRecoveryIntervalSeconds)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declara o exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _settings.ExchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            _logger.LogInformation(
                "? Conectado ao RabbitMQ | Host: {HostName}:{Port} | Exchange: {ExchangeName}",
                _settings.HostName,
                _settings.Port,
                _settings.ExchangeName
            );

            return _channel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro ao conectar ao RabbitMQ: {Message}", ex.Message);
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    /// <summary>
    /// Publica um evento genérico no RabbitMQ
    /// </summary>
    public async Task PublishAsync<T>(T evento, string routingKey, bool persistent = true) where T : class
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RabbitMQPublisher));

        ArgumentNullException.ThrowIfNull(evento);
        if (string.IsNullOrWhiteSpace(routingKey))
            throw new ArgumentException("Routing key não pode ser vazio", nameof(routingKey));

        try
        {
            var channel = await EnsureConnectionAsync();

            // Serializa o evento usando System.Text.Json (padrão .NET moderno)
            var message = JsonSerializer.Serialize(evento, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
                // NÃO usa JsonStringEnumConverter - serializa enums como números para compatibilidade
            });
            var body = Encoding.UTF8.GetBytes(message);

            // Configura propriedades da mensagem
            var properties = new BasicProperties
            {
                Persistent = persistent,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                MessageId = Guid.NewGuid().ToString()
            };

            // Publica a mensagem
            await channel.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body
            );

            _logger.LogDebug(
                "?? Evento publicado | Exchange: {Exchange} | RoutingKey: {RoutingKey} | MessageId: {MessageId}",
                _settings.ExchangeName,
                routingKey,
                properties.MessageId
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "? Erro ao publicar evento | RoutingKey: {RoutingKey} | Tipo: {EventType}",
                routingKey,
                typeof(T).Name
            );
            throw;
        }
    }

    /// <summary>
    /// Fecha a conexão de forma limpa
    /// </summary>
    private async Task CloseConnectionAsync()
    {
        try
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
                _channel = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _connection = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "?? Erro ao fechar conexão RabbitMQ");
        }
    }

    /// <summary>
    /// Dispose pattern implementation
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            CloseConnectionAsync().GetAwaiter().GetResult();
            _connectionLock.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer dispose do RabbitMQPublisher");
        }
        finally
        {
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }
}
