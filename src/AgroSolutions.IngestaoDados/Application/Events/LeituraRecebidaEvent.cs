using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Application.Events;

/// <summary>
/// Evento publicado quando uma leitura é recebida e registrada com sucesso
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
