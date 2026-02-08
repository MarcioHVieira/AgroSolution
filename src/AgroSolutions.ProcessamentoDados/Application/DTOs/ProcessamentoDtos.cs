using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Application.DTOs;

// ===== LEITURA PROCESSADA =====

public record LeituraProcessadaDto(
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
    DateTime TimestampRecebimento,
    DateTime TimestampProcessamento,
    QualidadeLeitura Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    StatusProcessamento Status,
    string? DadosAdicionais,
    string? MensagemErro
);

public record ConsultarLeiturasDto(
    Guid? SensorId = null,
    Guid? PropriedadeId = null,
    Guid? TalhaoId = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null,
    StatusProcessamento? Status = null,
    QualidadeLeitura? Qualidade = null,
    int? Pagina = 1,
    int? TamanhoPagina = 50
);

// ===== AGREGAÇÃO =====

public record AgregacaoDadosDto(
    Guid Id,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    TipoSensor TipoSensor,
    TipoAgregacao TipoAgregacao,
    DateTime PeriodoInicio,
    DateTime PeriodoFim,
    int TotalLeituras,
    decimal? ValorMinimo,
    decimal? ValorMaximo,
    decimal? ValorMedio,
    decimal? DesvioPadrao,
    string Unidade,
    int LeiturasNormais,
    int LeiturasSuspeitas,
    int LeiturasInvalidas
);

public record ConsultarAgregacoesDto(
    Guid? SensorId = null,
    Guid? PropriedadeId = null,
    Guid? TalhaoId = null,
    TipoAgregacao? TipoAgregacao = null,
    DateTime? DataInicio = null,
    DateTime? DataFim = null
);

public record GerarAgregacaoDto(
    Guid SensorId,
    TipoAgregacao TipoAgregacao,
    DateTime PeriodoInicio,
    DateTime? PeriodoFim = null
);

// ===== ESTATÍSTICAS =====

public record EstatisticasProcessamentoDto(
    DateTime PeriodoInicio,
    DateTime PeriodoFim,
    int TotalLeiturasProcessadas,
    int LeiturasComSucesso,
    int LeiturasComFalha,
    int LeiturasNormais,
    int LeiturasSuspeitas,
    int LeiturasInvalidas,
    decimal TaxaSucesso,
    TimeSpan TempoMedioProcessamento,
    int AgregacoesGeradas
);
