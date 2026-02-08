using AgroSolutions.IngestaoDados.Application.Events;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.SharedKernel.Messaging;

namespace AgroSolutions.IngestaoDados.Infrastructure.Services;

public class MensageriaService : IMensageriaService
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<MensageriaService> _logger;

    public MensageriaService(
        IRabbitMQPublisher publisher,
        ILogger<MensageriaService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task PublicarLeituraRecebidaAsync(LeituraRecebidaEvent evento)
    {
        try
        {
            _logger.LogInformation("MensageriaService: Publicando evento LeituraRecebida para {DeviceId}", evento.DeviceId);
            
            await _publisher.PublishAsync(evento, "leitura.recebida");
            
            _logger.LogInformation(
                "Evento LeituraRecebida publicado: Sensor={DeviceId}, Valor={Valor}{Unidade}",
                evento.DeviceId, evento.Valor, evento.Unidade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento LeituraRecebida para {DeviceId}", evento.DeviceId);
            throw;
        }
    }

    public async Task PublicarAlertaSensorAsync(AlertaSensorEvent evento)
    {
        try
        {
            await _publisher.PublishAsync(evento, $"sensor.alerta.{evento.TipoAlerta.ToString().ToLower()}");
            
            _logger.LogWarning(
                "Alerta de sensor publicado: Tipo={TipoAlerta}, Sensor={DeviceId}, Mensagem={Mensagem}",
                evento.TipoAlerta, evento.DeviceId, evento.Mensagem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar alerta de sensor");
            throw;
        }
    }
}
