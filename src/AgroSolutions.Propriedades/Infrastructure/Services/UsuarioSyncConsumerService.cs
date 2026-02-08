using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroSolutions.Propriedades.Infrastructure.Services;

/// <summary>
/// Consumer para sincronizar usuários via eventos do microserviço Identidade
/// Mantém Read Model local (UsuariosInfo) atualizado
/// </summary>
public class UsuarioSyncConsumerService : BackgroundService
{
    private readonly ILogger<UsuarioSyncConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ConsumerSettings _consumerSettings;
    private IConnection? _connection;
    private IChannel? _channel;

    public UsuarioSyncConsumerService(
        ILogger<UsuarioSyncConsumerService> logger,
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

        _logger.LogInformation("Iniciando UsuarioSyncConsumer...");

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

            // Exchange e fila para eventos de Usuário
            await _channel.ExchangeDeclareAsync("agrosolutions.identidade", "topic", durable: true);
            await _channel.QueueDeclareAsync("propriedades.usuario.sync", durable: true, exclusive: false, autoDelete: false);
            
            // Bind para eventos de usuário criado e atualizado
            await _channel.QueueBindAsync("propriedades.usuario.sync", "agrosolutions.identidade", "usuario.criado");
            await _channel.QueueBindAsync("propriedades.usuario.sync", "agrosolutions.identidade", "usuario.atualizado");

            _logger.LogInformation("UsuarioSyncConsumer aguardando eventos...");

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    
                    // System.Text.Json via helper do SharedKernel
                    var evento = RabbitMQMessageDeserializer.Deserialize<UsuarioEventoDto>(body);

                    if (evento != null)
                    {
                        await SincronizarUsuarioAsync(evento);
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Usuário {UsuarioId} sincronizado no Read Model", evento.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao sincronizar usuário");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync("propriedades.usuario.sync", false, consumer);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no UsuarioSyncConsumer");
        }
    }

    private async Task SincronizarUsuarioAsync(UsuarioEventoDto evento)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PropriedadesDbContext>();

        var usuarioInfo = await context.UsuariosInfo.FindAsync(evento.Id);

        if (usuarioInfo == null)
        {
            // Criar novo
            usuarioInfo = new UsuarioInfo
            {
                Id = evento.Id,
                Email = evento.Email,
                NomeCompleto = evento.NomeCompleto,
                DataSincronizacao = DateTime.UtcNow
            };
            context.UsuariosInfo.Add(usuarioInfo);
            _logger.LogInformation("Novo usuário adicionado ao Read Model: {Email}", evento.Email);
        }
        else
        {
            // Atualizar existente
            usuarioInfo.Email = evento.Email;
            usuarioInfo.NomeCompleto = evento.NomeCompleto;
            usuarioInfo.DataSincronizacao = DateTime.UtcNow;
            _logger.LogInformation("Usuário atualizado no Read Model: {Email}", evento.Email);
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
/// DTO do evento de Usuário publicado pelo microserviço Identidade
/// </summary>
internal record UsuarioEventoDto(
    Guid Id,
    string Email,
    string NomeCompleto
);
