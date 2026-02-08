using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Application.DTOs;

// ===== SENSOR =====

public record CriarSensorDto(
    Guid PropriedadeId,
    string DeviceId,
    string Nome,
    TipoSensor Tipo,
    int IntervaloLeituraMinutos = 15,
    Guid? TalhaoId = null,
    string? Fabricante = null,
    string? Modelo = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    decimal? Altitude = null,
    string? Observacoes = null
);

public record AtualizarSensorDto(
    string Nome,
    int IntervaloLeituraMinutos,
    Guid? TalhaoId = null,
    string? Fabricante = null,
    string? Modelo = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    decimal? Altitude = null,
    string? Observacoes = null
);

public record SensorDto(
    Guid Id,
    Guid PropriedadeId,
    Guid? TalhaoId,
    string DeviceId,
    string Nome,
    TipoSensor Tipo,
    string? Fabricante,
    string? Modelo,
    decimal? Latitude,
    decimal? Longitude,
    decimal? Altitude,
    int IntervaloLeituraMinutos,
    StatusSensor Status,
    DateTime? UltimaLeitura,
    DateTime? UltimaCalibracao,
    bool PrecisaCalibracao,
    string? Observacoes,
    DateTime DataCadastro,
    DateTime? DataAtualizacao
);

// ===== LEITURA =====

public record RegistrarLeituraDto(
    string DeviceId,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    int? NivelBateria = null,
    int? IntensidadeSinal = null,
    string? DadosAdicionais = null
);

public record RegistrarLeituraLoteDto(
    List<RegistrarLeituraDto> Leituras
);

public record LeituraSensorDto(
    Guid Id,
    Guid SensorId,
    string DeviceId,
    string NomeSensor,
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
    string? DadosAdicionais,
    string? Observacoes
);

public record EstatisticasLeituraDto(
    Guid SensorId,
    string DeviceId,
    string NomeSensor,
    TipoSensor TipoSensor,
    DateTime PeriodoInicio,
    DateTime PeriodoFim,
    int TotalLeituras,
    decimal? ValorMinimo,
    decimal? ValorMaximo,
    decimal? ValorMedio,
    int LeiturasNormais,
    int LeiturasSuspeitas,
    int LeiturasInvalidas
);

