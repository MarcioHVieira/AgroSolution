namespace AgroSolutions.IngestaoDados.Application.Events;

/// <summary>
/// Evento publicado quando um alerta relacionado a um sensor é detectado
/// </summary>
public record AlertaSensorEvent(
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    TipoAlerta TipoAlerta,
    string Mensagem,
    DateTime Timestamp
);

/// <summary>
/// Tipos de alertas relacionados a sensores
/// </summary>
public enum TipoAlerta
{
    BateriaBaixa = 1,
    SinalFraco = 2,
    SensorOffline = 3,
    ValorAnomalo = 4,
    CalibracaoNecessaria = 5
}
