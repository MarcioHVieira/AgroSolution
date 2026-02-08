using AgroSolutions.Notificacoes.Application.DTOs;
using AgroSolutions.Notificacoes.Application.Events;
using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Infrastructure.Data;
using AgroSolutions.Notificacoes.Infrastructure.Metrics;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.Notificacoes.Infrastructure.Services;

/// <summary>
/// Consumer para alertas de sensores (bateria baixa, sinal fraco, offline, etc.)
/// Envia notificações preventivas para o proprietário
/// </summary>
public class AlertaSensorConsumerService : BackgroundService
{
    private readonly ILogger<AlertaSensorConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public AlertaSensorConsumerService(
        ILogger<AlertaSensorConsumerService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IOptions<ConsumerSettings> consumerSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _consumerSettings = consumerSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(_consumerSettings.StartupDelaySeconds), stoppingToken);

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(_consumerSettings.NetworkRecoveryIntervalSeconds)
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        // Declara Dead Letter Exchange
        await _channel.ExchangeDeclareAsync(
            exchange: "agrosolutions.sensores.dlx",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        // Declara Dead Letter Queue
        await _channel.QueueDeclareAsync(
            queue: "alertas-sensores-dlq",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        await _channel.QueueBindAsync("alertas-sensores-dlq", "agrosolutions.sensores.dlx", "#");

        // Declara exchange principal
        await _channel.ExchangeDeclareAsync(
            exchange: "agrosolutions.ingestao",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        // Declara fila com DLX
        var args = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "agrosolutions.sensores.dlx" },
            { "x-dead-letter-routing-key", "sensor.alerta.dlq" },
            { "x-message-ttl", _consumerSettings.MessageTtlMilliseconds }
        };

        await _channel.QueueDeclareAsync(
            queue: "notificacoes.alertas-sensores",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args
        );

        // Bind para todos os alertas de sensor
        await _channel.QueueBindAsync("notificacoes.alertas-sensores", "agrosolutions.ingestao", "sensor.alerta.#");

        _logger.LogInformation("AlertaSensorConsumer conectado ao RabbitMQ");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            NotificacoesMetrics.NotificacoesEmProcessamento.Inc();

            try
            {
                var body = ea.Body.ToArray();
                
                // System.Text.Json
                var alertaSensor = RabbitMQMessageDeserializer.Deserialize<AlertaSensorEvent>(body);

                if (alertaSensor != null)
                {
                    await ProcessarAlertaSensorAsync(alertaSensor);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                NotificacoesMetrics.MensagensRabbitMQProcessadas.Inc();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar alerta de sensor");
                NotificacoesMetrics.MensagensRabbitMQComErro.Inc();
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            finally
            {
                NotificacoesMetrics.NotificacoesEmProcessamento.Dec();
            }
        };

        await _channel.BasicConsumeAsync("notificacoes.alertas-sensores", false, consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessarAlertaSensorAsync(AlertaSensorEvent alertaSensor)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificacaoService = scope.ServiceProvider.GetRequiredService<INotificacaoService>();
        var context = scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>();

        // Buscar proprietário da propriedade do Read Model local
        var propriedade = await context.PropriedadesInfo
            .Where(p => p.Id == alertaSensor.PropriedadeId)
            .FirstOrDefaultAsync();

        string emailDestinatario;
        string nomeDestinatario;
        Guid destinatarioId;

        if (propriedade != null)
        {
            emailDestinatario = propriedade.EmailProprietario;
            nomeDestinatario = propriedade.NomeProprietario;
            destinatarioId = propriedade.ProprietarioId;
            _logger.LogInformation("Proprietário encontrado para propriedade {PropriedadeId}: {Email}", 
                alertaSensor.PropriedadeId, emailDestinatario);
        }
        else
        {
            // Fallback: se não encontrou no Read Model, usar valores genéricos
            _logger.LogWarning("Proprietário da propriedade {PropriedadeId} não encontrado no Read Model. " +
                "Usando valores genéricos.", alertaSensor.PropriedadeId);
            emailDestinatario = "proprietario@agrosolutions.com.br";
            nomeDestinatario = "Proprietário";
            destinatarioId = Guid.Empty;
        }

        // Determinar prioridade baseada no tipo de alerta
        var prioridade = alertaSensor.TipoAlerta switch
        {
            "SensorOffline" => PrioridadeNotificacao.Urgente,
            "BateriaBaixa" => PrioridadeNotificacao.Alta,
            "CalibracaoNecessaria" => PrioridadeNotificacao.Normal,
            _ => PrioridadeNotificacao.Baixa
        };

        // Definir ícone do alerta
        var icone = alertaSensor.TipoAlerta switch
        {
            "BateriaBaixa" => "🔋",
            "SinalFraco" => "📶",
            "SensorOffline" => "🔴",
            "ValorAnomalo" => "⚠️",
            "CalibracaoNecessaria" => "🔧",
            _ => "⚡"
        };

        var assunto = $"[AgroSolutions] {icone} Alerta de Sensor - {alertaSensor.DeviceId}";
        var corpo = $@"
            <div style='font-family: Arial, sans-serif;'>
                <h2>{icone} Alerta de Sensor Detectado</h2>
                <p><strong>Sensor:</strong> {alertaSensor.DeviceId}</p>
                <p><strong>Tipo:</strong> {alertaSensor.TipoAlerta}</p>
                <p><strong>Mensagem:</strong> {alertaSensor.Mensagem}</p>
                <p><strong>Data/Hora:</strong> {alertaSensor.Timestamp:dd/MM/yyyy HH:mm:ss}</p>
                
                <hr>
                <h3>Ações Recomendadas:</h3>
                {ObterRecomendacao(alertaSensor.TipoAlerta)}
                
                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                    Este é um alerta automático do sistema AgroSolutions.<br>
                    Acesse o dashboard para mais detalhes.
                </p>
            </div>
        ";

        NotificacoesMetrics.NotificacoesCriadas.WithLabels("Email", prioridade.ToString()).Inc();

        await notificacaoService.CriarAsync(new CriarNotificacaoDto(
            AlertaId: Guid.NewGuid(), // AlertaSensor não tem AlertaId
            TalhaoId: Guid.Empty, // AlertaSensor não está vinculado a talhão
            DestinatarioId: destinatarioId,
            EmailDestinatario: emailDestinatario,
            NomeDestinatario: nomeDestinatario,
            Tipo: TipoNotificacao.Email,
            Prioridade: prioridade,
            Assunto: assunto,
            Mensagem: corpo
        ));

        _logger.LogInformation("Notificação criada para alerta de sensor: {DeviceId} - {TipoAlerta}", 
            alertaSensor.DeviceId, alertaSensor.TipoAlerta);
    }

    private static string ObterRecomendacao(string tipoAlerta)
    {
        return tipoAlerta switch
        {
            "BateriaBaixa" => "<p>🔋 <strong>Trocar ou recarregar a bateria do sensor imediatamente</strong> para evitar perda de dados.</p>",
            "SinalFraco" => "<p>📶 Verificar posicionamento do sensor e gateway de comunicação. Considere aproximar o gateway ou instalar repetidor de sinal.</p>",
            "SensorOffline" => "<p>🔴 <strong>URGENTE:</strong> Sensor não está respondendo. Verifique conexão, alimentação e integridade física do dispositivo.</p>",
            "ValorAnomalo" => "<p>⚠️ Valores fora do padrão detectados. Considere calibração ou inspeção física do sensor.</p>",
            "CalibracaoNecessaria" => "<p>🔧 Agende calibração do sensor para manter precisão das leituras.</p>",
            _ => "<p>Verifique o sensor no painel de controle.</p>"
        };
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AlertaSensorConsumer parando...");

        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        await base.StopAsync(cancellationToken);
    }
}
