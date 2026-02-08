using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Application.Events;

/// <summary>
/// Evento recebido quando uma leitura é registrada no microserviço IngestaoDados
/// </summary>
public record LeituraRecebidaEvent(
    Guid Id,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    TipoSensor TipoSensor,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    DateTime TimestampRecebimento,
    QualidadeLeitura Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    bool BateriaBaixa,
    bool SinalFraco,
    TimeSpan LatenciaRecebimento,
    string? DadosAdicionais
);
