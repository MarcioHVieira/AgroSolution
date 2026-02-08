using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.Notificacoes.Infrastructure.Services;

/// <summary>
/// Consumer para sincronizar propriedades via eventos do microserviço Propriedades
/// Mantém Read Model local (PropriedadesInfo) atualizado
/// </summary>
public class PropriedadeSyncConsumerService : BackgroundService
{
    private readonly ILogger<PropriedadeSyncConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public PropriedadeSyncConsumerService(
        ILogger<PropriedadeSyncConsumerService> logger,
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

        _logger.LogInformation("Iniciando PropriedadeSyncConsumer...");

        try
        {
            var rabbitHost = _configuration["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory
            {
                HostName = rabbitHost,
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest",
                AutomaticRecoveryEnabled = true
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Exchange e fila para eventos de Propriedade
            await _channel.ExchangeDeclareAsync("agrosolutions.propriedades", "topic", durable: true);
            await _channel.QueueDeclareAsync("notificacoes.propriedade.sync", durable: true, exclusive: false, autoDelete: false);
            
            // Bind para eventos de propriedade criada e atualizada
            await _channel.QueueBindAsync("notificacoes.propriedade.sync", "agrosolutions.propriedades", "propriedade.criada");
            await _channel.QueueBindAsync("notificacoes.propriedade.sync", "agrosolutions.propriedades", "propriedade.atualizada");

            _logger.LogInformation("PropriedadeSyncConsumer aguardando eventos...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    
                    var evento = RabbitMQMessageDeserializer.Deserialize<PropriedadeEventoDto>(body);

                    if (evento != null)
                    {
                        await SincronizarPropriedadeAsync(evento);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Propriedade {PropriedadeId} sincronizada no Read Model", evento.PropriedadeId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar propriedade");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync("notificacoes.propriedade.sync", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no PropriedadeSyncConsumer");
        }
    }

    private async Task SincronizarPropriedadeAsync(PropriedadeEventoDto evento)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NotificacoesDbContext>();

        var propriedadeInfo = await context.PropriedadesInfo.FindAsync(evento.PropriedadeId);

        if (propriedadeInfo == null)
        {
            // Criar novo
            propriedadeInfo = new PropriedadeInfo
            {
                Id = evento.PropriedadeId,
                ProprietarioId = evento.ProprietarioId,
                EmailProprietario = evento.EmailProprietario,
                NomeProprietario = evento.NomeProprietario,
                DataSincronizacao = DateTime.UtcNow
            };
            context.PropriedadesInfo.Add(propriedadeInfo);
            _logger.LogInformation("Nova propriedade adicionada ao Read Model: {PropriedadeId}", evento.PropriedadeId);
        }
        else
        {
            // Atualizar existente
            propriedadeInfo.ProprietarioId = evento.ProprietarioId;
            propriedadeInfo.EmailProprietario = evento.EmailProprietario;
            propriedadeInfo.NomeProprietario = evento.NomeProprietario;
            propriedadeInfo.DataSincronizacao = DateTime.UtcNow;
            _logger.LogInformation("Propriedade atualizada no Read Model: {PropriedadeId}", evento.PropriedadeId);
        }

        await context.SaveChangesAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// DTO do evento de Propriedade publicado pelo microserviço Propriedades
/// </summary>
internal record PropriedadeEventoDto(
    Guid PropriedadeId,
    string Nome,
    Guid ProprietarioId,
    string EmailProprietario,
    string NomeProprietario
);
