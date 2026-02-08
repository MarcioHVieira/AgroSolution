namespace AgroSolutions.Notificacoes.Application.Events;

/// <summary>
/// Evento consumido quando há problema com sensor (bateria baixa, sinal fraco, etc.)
/// </summary>
public record AlertaSensorEvent(
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    string TipoAlerta,
    string Mensagem,
    DateTime Timestamp
);
