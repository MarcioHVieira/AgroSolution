namespace AgroSolutions.Analise.Application.Events;

/// <summary>
/// Evento recebido quando dados processados estão disponíveis para análise
/// </summary>
public record DadosProcessadosEvent(
    Guid Id,
    Guid SensorId,
    Guid TalhaoId,
    decimal UmidadeSolo,
    decimal Temperatura,
    decimal Precipitacao,
    DateTime DataHoraLeitura,
    DateTime DataHoraProcessamento
);
