using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Application.Events;

/// <summary>
/// Evento publicado quando dados são processados e agregados com sucesso
/// </summary>
public record DadosProcessadosEvent(
    Guid Id,
    Guid LeituraOrigemId,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    TipoSensor TipoSensor,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    DateTime TimestampProcessamento,
    QualidadeLeitura Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    string? DadosAdicionais
);
