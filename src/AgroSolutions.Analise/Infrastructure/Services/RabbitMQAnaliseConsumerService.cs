using AgroSolutions.Analise.Application.Interfaces;
using AgroSolutions.Analise.Infrastructure.Metrics;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using AnaliseSettings = AgroSolutions.Analise.Configuration.Settings;

namespace AgroSolutions.Analise.Infrastructure.Services;

/// <summary>
/// Serviço de consumo de mensagens do RabbitMQ para análise de dados
/// </summary>
public class RabbitMQAnaliseConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQAnaliseConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AnaliseSettings.RabbitMQSettings _rabbitMQSettings;
    private readonly AnaliseSettings.MotorRegrasSettings _motorRegrasSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQAnaliseConsumerService(
        ILogger<RabbitMQAnaliseConsumerService> logger,
        IServiceProvider serviceProvider,
        IOptions<AnaliseSettings.RabbitMQSettings> rabbitMQSettings,
        IOptions<AnaliseSettings.MotorRegrasSettings> motorRegrasSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _rabbitMQSettings = rabbitMQSettings.Value;
        _motorRegrasSettings = motorRegrasSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_motorRegrasSettings.StartupDelaySeconds), stoppingToken);

            var factory = new ConnectionFactory
            {
                HostName = _rabbitMQSettings.HostName,
                UserName = _rabbitMQSettings.UserName,
                Password = _rabbitMQSettings.Password,
                AutomaticRecoveryEnabled = _rabbitMQSettings.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_rabbitMQSettings.NetworkRecoveryIntervalSeconds)
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declarar exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _rabbitMQSettings.ExchangeName,
                type: _rabbitMQSettings.ExchangeType,
                durable: _rabbitMQSettings.Durable,
                autoDelete: _rabbitMQSettings.AutoDelete);

            // Declarar fila
            await _channel.QueueDeclareAsync(
                queue: _rabbitMQSettings.QueueName,
                durable: _rabbitMQSettings.Durable,
                exclusive: _rabbitMQSettings.Exclusive,
                autoDelete: _rabbitMQSettings.AutoDelete);

            // Bind fila ao exchange
            await _channel.QueueBindAsync(
                queue: _rabbitMQSettings.QueueName,
                exchange: _rabbitMQSettings.ExchangeName,
                routingKey: _rabbitMQSettings.RoutingKey);

            // Configurar QoS
            await _channel.BasicQosAsync(
                _rabbitMQSettings.QoS.PrefetchSize,
                _rabbitMQSettings.QoS.PrefetchCount,
                _rabbitMQSettings.QoS.Global);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                AnaliseMetrics.MensagensRabbitMQRecebidas.Inc();
                
                try
                {
                    var body = ea.Body.ToArray();

                    _logger.LogDebug("Mensagem recebida");

                    var leitura = RabbitMQMessageDeserializer.Deserialize<LeituraProcessadaMessage>(body);

                    if (leitura != null && leitura.TalhaoId.HasValue)
                    {
                        await ProcessarLeituraAsync(leitura);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        AnaliseMetrics.MensagensRabbitMQProcessadas.Inc();
                        _logger.LogInformation("Leitura processada e analisada: TalhaoId={TalhaoId}", leitura.TalhaoId);
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao deserializar mensagem ou TalhaoId não informado");
                        AnaliseMetrics.MensagensRabbitMQComErro.Inc();
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem");
                    AnaliseMetrics.MensagensRabbitMQComErro.Inc();
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true); // Requeue
                }
            };

            await _channel.BasicConsumeAsync(_rabbitMQSettings.QueueName, false, consumer);

            _logger.LogWarning("Consumer registrado - Aguardando mensagens na fila '{Queue}'", _rabbitMQSettings.QueueName);

            // Manter serviço rodando
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("RabbitMQ Analise Consumer Service encerrando gracefully...");
            }
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("RabbitMQ Analise Consumer Service cancelado durante inicialização");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("RabbitMQ Analise Consumer cancelado (OperationCanceledException)");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Erro FATAL no RabbitMQ Analise Consumer: {Message}", ex.Message);
            throw;
        }
        finally
        {
            _logger.LogWarning("RabbitMQ Analise Consumer Service FINALIZANDO");
        }
    }

    private async Task ProcessarLeituraAsync(LeituraProcessadaMessage leitura)
    {
        using var scope = _serviceProvider.CreateScope();
        var motorRegras = scope.ServiceProvider.GetRequiredService<IMotorRegrasService>();

        try
        {
            if (leitura.TalhaoId.HasValue)
            {
                using (AnaliseMetrics.TempoAvaliacaoRegra.WithLabels("todas"))
                {
                    // Processar leitura e avaliar regras
                    var leituraDto = new LeituraParaAnaliseDto(
                        TalhaoId: leitura.TalhaoId.Value,
                        TipoSensor: leitura.TipoSensor,
                        Valor: leitura.Valor,
                        TimestampLeitura: leitura.TimestampLeitura
                    );

                    await motorRegras.ProcessarLeituraEAvaliarRegrasAsync(leituraDto);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao avaliar regras para TalhaoId={TalhaoId}", leitura.TalhaoId);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ Analise Consumer Service parando...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }
        
        if (_connection != null)
        {
            await _connection.CloseAsync();
        }
        
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Mensagem de leitura processada recebida do RabbitMQ
/// </summary>
internal record LeituraProcessadaMessage(
    Guid Id,
    Guid LeituraOrigemId,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    int TipoSensor,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    DateTime TimestampProcessamento,
    int Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    string? DadosAdicionais
);
