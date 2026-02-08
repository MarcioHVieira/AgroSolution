using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Events;
using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.ProcessamentoDados.Infrastructure.Metrics;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.ProcessamentoDados.Infrastructure.Services;

/// <summary>
/// Background service que consome mensagens do RabbitMQ
/// </summary>
public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel _channel = null!;

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        IOptions<ConsumerSettings> consumerSettings)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
        _consumerSettings = consumerSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(_consumerSettings.StartupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || _connection.IsOpen == false)
                    {
                        await ConectarRabbitMQAsync();
                        _logger.LogWarning("Conectado ao RabbitMQ com sucesso!");
                    }

                    // Aguarda cancelamento
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("RabbitMQ Consumer Service está parando...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no loop do RabbitMQ Consumer: {Message}", ex.Message);
                    await Task.Delay(10000, stoppingToken); // Aguarda 10s antes de reconectar
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ExecuteAsync cancelado durante delay inicial");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "ERRO FATAL no ExecuteAsync: {Message}", ex.Message);
            throw;
        }
        finally
        {
            _logger.LogWarning("RabbitMQ Consumer Service FINALIZANDO");
        }
    }

    private async Task ConectarRabbitMQAsync()
    {
        try
        {
            var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672");
            var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var exchangeName = _configuration["RabbitMQ:ExchangeName"] ?? "agrosolutions.ingestaodados";
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "processamento.leituras";
            var routingKey = _configuration["RabbitMQ:RoutingKey"] ?? "leitura.recebida";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(_consumerSettings.NetworkRecoveryIntervalSeconds)
            };


            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();


            await _channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false
            );

            // Declara fila
            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            // Bind da fila ao exchange
            await _channel.QueueBindAsync(
                queue: queueName,
                exchange: exchangeName,
                routingKey: routingKey
            );

            // Configura QoS (prefetch)
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await ProcessarMensagemAsync(ea);
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro ao conectar ao RabbitMQ: {Message}", ex.Message);
            throw;
        }
    }

    private async Task ProcessarMensagemAsync(BasicDeliverEventArgs ea)
    {
        ProcessamentoDadosMetrics.MensagensRabbitMQRecebidas.Inc();
        ProcessamentoDadosMetrics.LeiturasEmProcessamento.Inc();
        
        try
        {
            var body = ea.Body.ToArray();
            
            _logger.LogDebug("Mensagem recebida");

            // System.Text.Json via helper do SharedKernel
            var evento = RabbitMQMessageDeserializer.Deserialize<LeituraRecebidaEvent>(body);

            if (evento == null)
            {
                _logger.LogWarning("Mensagem inválida recebida");
                ProcessamentoDadosMetrics.MensagensRabbitMQComErro.Inc();
                await _channel!.BasicNackAsync(ea.DeliveryTag, false, false); // Descarta mensagem
                return;
            }

            // Processa evento usando o service
            using (ProcessamentoDadosMetrics.TempoProcessamentoLeitura.WithLabels(evento.Unidade))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var processamentoService = scope.ServiceProvider.GetRequiredService<IProcessamentoService>();
                
                await processamentoService.ProcessarLeituraAsync(evento);

                ProcessamentoDadosMetrics.LeiturasProcessadas.WithLabels(evento.Unidade).Inc();
                ProcessamentoDadosMetrics.ValoresProcessados.WithLabels(evento.Unidade, evento.Unidade).Observe((double)evento.Valor);
            }

            // Confirma processamento
            await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            ProcessamentoDadosMetrics.MensagensRabbitMQProcessadas.Inc();
            
            _logger.LogInformation("Leitura processada: {DeviceId} - {Valor}{Unidade}", 
                evento.DeviceId, evento.Valor, evento.Unidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem: {Message}", ex.Message);
            ProcessamentoDadosMetrics.LeiturasComErro.WithLabels(ex.GetType().Name).Inc();
            ProcessamentoDadosMetrics.MensagensRabbitMQComErro.Inc();
            
            // Rejeita mensagem e coloca na fila novamente (retry)
            await _channel!.BasicNackAsync(ea.DeliveryTag, false, true);
        }
        finally
        {
            ProcessamentoDadosMetrics.LeiturasEmProcessamento.Dec();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? Parando RabbitMQ Consumer Service...");
        
        if (_channel != null)
        {
            await _channel.CloseAsync();
            _channel.Dispose();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
