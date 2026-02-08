using AgroSolutions.Notificacoes.Application.DTOs;
using AgroSolutions.Notificacoes.Application.Events;
using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Infrastructure.Metrics;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.Notificacoes.Infrastructure.Services;

public class RabbitMQNotificacoesConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQNotificacoesConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQNotificacoesConsumerService(ILogger<RabbitMQNotificacoesConsumerService> logger, 
        IServiceProvider serviceProvider, IConfiguration configuration, IOptions<ConsumerSettings> consumerSettings)
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
            exchange: "agrosolutions.alertas.dlx",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        );

        // Declara Dead Letter Queue
        await _channel.QueueDeclareAsync(
            queue: "alertas-notificacoes.dlq",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        // Bind DLQ ao DLX
        await _channel.QueueBindAsync(
            queue: "alertas-notificacoes.dlq",
            exchange: "agrosolutions.alertas.dlx",
            routingKey: "alerta.#"
        );

        // Declara fila principal com DLX configurado
        var queueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", "agrosolutions.alertas.dlx" },
            { "x-dead-letter-routing-key", "alerta.dlq" },
            { "x-max-priority", 10 } // Suporte a prioridades
        };


        await _channel.QueueDeclareAsync(
            queue: "alertas-notificacoes",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs
        );

        // BIND DA FILA AO EXCHANGE
        await _channel.QueueBindAsync(
            queue: "alertas-notificacoes",
            exchange: "agrosolutions.alertas",
            routingKey: "alerta.#"  // Recebe todos os alertas (qualquer severidade/tipo)
        );

        // Configura QoS - processa 1 mensagem por vez
        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

        _logger.LogInformation("RabbitMQ Notificações Consumer configurado - Fila: alertas-notificacoes, Exchange: agrosolutions.alertas");
        _logger.LogInformation("Aguardando alertas com routing key: alerta.#");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            NotificacoesMetrics.MensagensRabbitMQRecebidas.Inc();
            NotificacoesMetrics.NotificacoesEmProcessamento.Inc();
            
            try
            {
                var body = ea.Body.ToArray();
                
                var alerta = RabbitMQMessageDeserializer.Deserialize<AlertaGeradoEvent>(body);

                if (alerta != null)
                {
                    using (NotificacoesMetrics.TempoProcessamentoNotificacao.WithLabels("email"))
                    {
                        await CriarNotificacaoDeAlertaAsync(alerta);
                    }
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                NotificacoesMetrics.MensagensRabbitMQProcessadas.Inc();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar alerta");
                NotificacoesMetrics.MensagensRabbitMQComErro.Inc();
                await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
            finally
            {
                NotificacoesMetrics.NotificacoesEmProcessamento.Dec();
            }
        };

        await _channel.BasicConsumeAsync("alertas-notificacoes", false, consumer);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task CriarNotificacaoDeAlertaAsync(AlertaGeradoEvent alerta)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificacaoService = scope.ServiceProvider.GetRequiredService<INotificacaoService>();

        if (string.IsNullOrEmpty(alerta.EmailDestinatario))
        {
            _logger.LogWarning("Alerta {AlertaId} sem destinatário. Notificação não será enviada.", alerta.AlertaId);
            return;
        }

        // Verificar se já existe notificação para este alerta (previne duplicação)
        var notificacoesExistentes = await notificacaoService.ObterTodasAsync();
        if (notificacoesExistentes.Any(n => n.AlertaId == alerta.AlertaId))
        {
            _logger.LogInformation("Notificação para o alerta {AlertaId} já existe. Ignorando duplicação.", alerta.AlertaId);
            return;
        }

        var prioridade = alerta.Severidade switch
        {
            NivelSeveridade.Critico => PrioridadeNotificacao.Urgente,
            NivelSeveridade.Alto => PrioridadeNotificacao.Alta,
            NivelSeveridade.Medio => PrioridadeNotificacao.Normal,
            _ => PrioridadeNotificacao.Baixa
        };

        NotificacoesMetrics.NotificacoesCriadas.WithLabels("Email", prioridade.ToString()).Inc();

        var assunto = $"[AgroSolutions] {alerta.Titulo}";
        var corpo = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF5722; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .alert {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .danger {{ background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌾 AgroSolutions - Alerta</h1>
        </div>
        <div class='content'>
            <h2>{alerta.Titulo}</h2>
            <p><strong>Severidade:</strong> {alerta.Severidade}</p>
            <p>{alerta.Mensagem}</p>
            {(!string.IsNullOrEmpty(alerta.Recomendacao) ? $@"
            <div class='alert'>
                <h3>💡 Recomendação:</h3>
                <p>{alerta.Recomendacao}</p>
            </div>" : "")}
            <p><small>Data/Hora: {alerta.DataGeracao:dd/MM/yyyy HH:mm}</small></p>
        </div>
        <div class='footer'>
            <p>Atenciosamente,<br>Equipe AgroSolutions</p>
            <p>Este é um e-mail automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";

        await notificacaoService.CriarAsync(new CriarNotificacaoDto(
            alerta.AlertaId, 
            alerta.TalhaoId, 
            alerta.DestinatarioId ?? Guid.Empty, 
            alerta.EmailDestinatario, 
            alerta.NomeDestinatario ?? "Usuário",
            TipoNotificacao.Email, 
            prioridade, 
            assunto, 
            corpo
        ));

        _logger.LogInformation("Notificação criada para {Email} - Alerta: {Titulo}", alerta.EmailDestinatario, alerta.Titulo);
    }
}
