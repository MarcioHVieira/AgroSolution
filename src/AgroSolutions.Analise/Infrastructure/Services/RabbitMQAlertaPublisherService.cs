using RabbitMQ.Client;
using System.Text.Json;

namespace AgroSolutions.Analise.Infrastructure.Services;

/// <summary>
/// Serviço para publicar alertas no RabbitMQ
/// </summary>
public class RabbitMQAlertaPublisherService : IRabbitMQAlertaPublisherService
{
    private readonly ILogger<RabbitMQAlertaPublisherService> _logger;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed;
    private int _mensagensPublicadas;
    private int _falhasPublicacao;

    public RabbitMQAlertaPublisherService(
        ILogger<RabbitMQAlertaPublisherService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _ = InicializarConexaoAsync();
    }

    private async Task InicializarConexaoAsync()
    {
        var tentativa = 0;
        var maxTentativas = int.Parse(_configuration["RabbitMQ:MaxRetries"] ?? "5");
        var delayInicial = double.Parse(_configuration["RabbitMQ:RetryDelaySeconds"] ?? "2");

        while (tentativa < maxTentativas && !_disposed)
        {
            try
            {
                await _connectionLock.WaitAsync();
                try
                {
                    if (_connection?.IsOpen == true)
                        return;

                    var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
                    var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
                    var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
                    var password = _configuration["RabbitMQ:Password"] ?? "guest";
                    var exchangeAlertas = _configuration["RabbitMQ:ExchangeAlertas"] ?? "agrosolutions.alertas";

                    var factory = new ConnectionFactory
                    {
                        HostName = hostName,
                        Port = port,
                        UserName = userName,
                        Password = password,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        RequestedHeartbeat = TimeSpan.FromSeconds(60),
                        RequestedConnectionTimeout = TimeSpan.FromSeconds(5) // Timeout de conexão
                    };

                    _connection = await factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();

                    // Declara Dead Letter Exchange
                    await _channel.ExchangeDeclareAsync(
                        exchange: $"{exchangeAlertas}.dlx",
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false
                    );

                    // Declara exchange principal de alertas
                    await _channel.ExchangeDeclareAsync(
                        exchange: exchangeAlertas,
                        type: ExchangeType.Topic,
                        durable: true,
                        autoDelete: false
                    );

                    _logger.LogInformation("Conectado ao RabbitMQ Alerta Publisher: {HostName}:{Port}", hostName, port);
                    return;
                }
                finally
                {
                    _connectionLock.Release();
                }
            }
            catch (Exception ex)
            {
                tentativa++;
                var delay = TimeSpan.FromSeconds(delayInicial * Math.Pow(2, tentativa - 1)); // Backoff exponencial
                
                _logger.LogWarning(ex, 
                    "Falha ao conectar ao RabbitMQ (tentativa {Tentativa}/{MaxTentativas}). Tentando novamente em {Delay}s",
                    tentativa, maxTentativas, delay.TotalSeconds);

                if (tentativa < maxTentativas)
                    await Task.Delay(delay);
                else
                    _logger.LogError(ex, "Erro ao conectar ao RabbitMQ após {MaxTentativas} tentativas", maxTentativas);
            }
        }
    }

    /// <summary>
    /// Publica um alerta no RabbitMQ com prioridade e TTL
    /// </summary>
    public async Task<bool> PublicarAlertaAsync<T>(T alerta, string routingKey, int prioridade = 5, int ttlMinutos = 60) where T : class
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQAlertaPublisherService));
        }

        try
        {
            // Reconecta se necessário
            if (_channel == null || _channel.IsClosed)
            {
                await InicializarConexaoAsync();
            }

            if (_channel == null || _channel.IsClosed)
            {
                _logger.LogError("Canal RabbitMQ não está disponível");
                _falhasPublicacao++;
                return false;
            }

            var exchangeAlertas = _configuration["RabbitMQ:ExchangeAlertas"] ?? "agrosolutions.alertas";
            
            // System.Text.Json
            var message = JsonSerializer.Serialize(alerta);
            var body = System.Text.Encoding.UTF8.GetBytes(message);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                Priority = (byte)Math.Clamp(prioridade, 0, 10), // Prioridade de 0 a 10
                Expiration = TimeSpan.FromMinutes(ttlMinutos).TotalMilliseconds.ToString() // TTL
            };

            // Adiciona headers customizados
            properties.Headers = new Dictionary<string, object?>
            {
                { "x-source", "AgroSolutions.Analise" },
                { "x-published-at", DateTime.UtcNow.ToString("O") },
                { "x-routing-key", routingKey }
            };

            await _channel.BasicPublishAsync(
                exchange: exchangeAlertas,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body
            );

            _mensagensPublicadas++;
            _logger.LogInformation(
                "Alerta publicado - Exchange: {Exchange}, RoutingKey: {RoutingKey}, Prioridade: {Prioridade}, TTL: {TTL}min",
                exchangeAlertas, routingKey, prioridade, ttlMinutos);

            // Log de métricas a cada 100 mensagens
            if (_mensagensPublicadas % 100 == 0)
            {
                _logger.LogInformation(
                    "Métricas Publisher - Publicadas: {Publicadas}, Falhas: {Falhas}, Taxa de Sucesso: {Taxa}%",
                    _mensagensPublicadas, _falhasPublicacao, 
                    _mensagensPublicadas > 0 ? ((_mensagensPublicadas - _falhasPublicacao) * 100.0 / _mensagensPublicadas) : 0);
            }

            return true;
        }
        catch (Exception ex)
        {
            _falhasPublicacao++;
            _logger.LogError(ex, "Erro ao publicar alerta: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Publica alerta crítico com alta prioridade e TTL curto
    /// </summary>
    public async Task<bool> PublicarAlertaCriticoAsync<T>(T alerta, string routingKey) where T : class
    {
        return await PublicarAlertaAsync(alerta, routingKey, prioridade: 10, ttlMinutos: 30);
    }

    /// <summary>
    /// Publica alerta normal com prioridade média
    /// </summary>
    public async Task<bool> PublicarAlertaNormalAsync<T>(T alerta, string routingKey) where T : class
    {
        return await PublicarAlertaAsync(alerta, routingKey, prioridade: 5, ttlMinutos: 120);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _logger.LogInformation(
                "Desconectando RabbitMQ Publisher - Total publicado: {Total}, Falhas: {Falhas}",
                _mensagensPublicadas, _falhasPublicacao);

            _channel?.CloseAsync().Wait();
            _channel?.Dispose();
            _connection?.CloseAsync().Wait();
            _connection?.Dispose();
            _connectionLock.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar RabbitMQ Publisher");
        }

        GC.SuppressFinalize(this);
    }
}
