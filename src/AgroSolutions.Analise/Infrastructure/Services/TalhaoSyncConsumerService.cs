using AgroSolutions.Analise.Infrastructure.Data;
using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.Analise.Infrastructure.Services;

/// <summary>
/// Consumer para sincronizar informações de Talhões via eventos do microserviço Propriedades
/// </summary>
public class TalhaoSyncConsumerService : BackgroundService
{
    private readonly ILogger<TalhaoSyncConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public TalhaoSyncConsumerService(
        ILogger<TalhaoSyncConsumerService> logger,
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
        var instanceId = Guid.NewGuid().ToString("N").Substring(0, 8);
        
        await Task.Delay(TimeSpan.FromSeconds(_consumerSettings.StartupDelaySeconds), stoppingToken);

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

            // Exchange e fila para eventos de Talhão
            await _channel.ExchangeDeclareAsync("agrosolutions.propriedades", "topic", durable: true);
            await _channel.QueueDeclareAsync("analise.talhao.sync", durable: true, exclusive: false, autoDelete: false);
            
            // Bind para eventos de talhão criado e atualizado
            await _channel.QueueBindAsync("analise.talhao.sync", "agrosolutions.propriedades", "talhao.criado");
            await _channel.QueueBindAsync("analise.talhao.sync", "agrosolutions.propriedades", "talhao.atualizado");

            _logger.LogInformation("TalhaoSyncConsumer [{InstanceId}] conectado - Aguardando eventos...", instanceId);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    
                    _logger.LogInformation("[{InstanceId}] Evento recebido - RoutingKey: {RoutingKey}, DeliveryTag: {DeliveryTag}", 
                        instanceId, ea.RoutingKey, ea.DeliveryTag);
                    
                    var evento = RabbitMQMessageDeserializer.Deserialize<TalhaoCriadoEventDto>(body);

                    if (evento != null)
                    {
                        await SincronizarTalhaoAsync(evento);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("[{InstanceId}] Talhão {TalhaoId} - {Nome} sincronizado e ACK enviado", 
                            instanceId, evento.TalhaoId, evento.Nome);
                    }
                    else
                    {
                        _logger.LogWarning("[{InstanceId}] Evento de talhão nulo, descartando mensagem", instanceId);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[{InstanceId}] Erro ao sincronizar talhão: {Message}", instanceId, ex.Message);
                    
                    // NÃO faz requeue para evitar loop infinito
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync("analise.talhao.sync", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no Talhao Sync Consumer");
        }
    }

    private async Task SincronizarTalhaoAsync(TalhaoCriadoEventDto evento)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnaliseDbContext>();

        try
        {
            // IDEMPOTÊNCIA: Usar FindAsync que aproveita cache do EF Core
            var talhaoInfo = await context.TalhoesInfo.FindAsync(evento.TalhaoId);

            if (talhaoInfo == null)
            {
                // Criar novo
                talhaoInfo = new TalhaoInfo
                {
                    Id = evento.TalhaoId,
                    Nome = evento.Nome,
                    PropriedadeId = evento.PropriedadeId,
                    ProprietarioId = evento.ProprietarioId,
                    EmailProprietario = evento.EmailProprietario,
                    NomeProprietario = evento.NomeProprietario,
                    DataSincronizacao = DateTime.UtcNow
                };
                context.TalhoesInfo.Add(talhaoInfo);
                _logger.LogInformation("Criando novo TalhaoInfo: {TalhaoId} - {Nome}", evento.TalhaoId, evento.Nome);
            }
            else
            {
                // Atualizar existente
                talhaoInfo.Nome = evento.Nome;
                talhaoInfo.PropriedadeId = evento.PropriedadeId;
                talhaoInfo.ProprietarioId = evento.ProprietarioId;
                talhaoInfo.EmailProprietario = evento.EmailProprietario;
                talhaoInfo.NomeProprietario = evento.NomeProprietario;
                talhaoInfo.DataSincronizacao = DateTime.UtcNow;
                _logger.LogInformation("Atualizando TalhaoInfo existente: {TalhaoId} - {Nome}", evento.TalhaoId, evento.Nome);
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("TalhaoInfo {TalhaoId} salvo com sucesso", evento.TalhaoId);
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate key") == true)
        {
            // Violação de chave primária - registro já existe (race condition)
            _logger.LogWarning("TalhaoInfo {TalhaoId} já existe (duplicate key), ignorando", evento.TalhaoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao sincronizar TalhaoInfo {TalhaoId}", evento.TalhaoId);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// DTO do evento TalhaoCriadoEvent publicado pelo microserviço Propriedades
/// </summary>
internal record TalhaoCriadoEventDto(
    Guid TalhaoId,
    Guid PropriedadeId,
    string Nome,
    decimal AreaHectares,
    string Cultura,
    string Status,
    DateTime DataCriacao,
    Guid ProprietarioId,
    string EmailProprietario,
    string NomeProprietario
);
