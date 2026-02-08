using AgroSolutions.Analise.Domain.Enums;

namespace AgroSolutions.Analise.Application.DTOs;

/// <summary>
/// DTO para criação de alerta
/// </summary>
public record CriarAlertaDto(
    Guid TalhaoId,
    TipoAlerta Tipo,
    NivelSeveridade Severidade,
    string Titulo,
    string Mensagem,
    string? Recomendacao,
    decimal? ValorReferencia
);

/// <summary>
/// DTO de resposta de alerta
/// </summary>
public record AlertaDto(
    Guid Id,
    Guid TalhaoId,
    TipoAlerta Tipo,
    string TipoNome,
    NivelSeveridade Severidade,
    string SeveridadeNome,
    StatusAlerta Status,
    string StatusNome,
    string Titulo,
    string Mensagem,
    string? Recomendacao,
    DateTime DataGeracao,
    DateTime? DataVisualizacao,
    DateTime? DataResolucao,
    decimal? ValorReferencia
);

/// <summary>
/// DTO para atualizar status do alerta
/// </summary>
public record AtualizarStatusAlertaDto(
    StatusAlerta NovoStatus
);

/// <summary>
/// DTO para dados de leitura recebidos do ProcessamentoDados via RabbitMQ
/// </summary>
public record LeituraProcessadaDto(
    Guid Id,
    Guid SensorId,
    Guid TalhaoId,
    decimal UmidadeSolo,
    decimal Temperatura,
    decimal Precipitacao,
    DateTime DataHoraLeitura,
    DateTime DataHoraProcessamento
);

/// <summary>
/// DTO para configuração de regra de alerta
/// </summary>
public record CriarRegraAlertaDto(
    string Nome,
    string? Descricao,
    TipoAlerta TipoAlerta,
    NivelSeveridade Severidade,
    bool Ativa,
    string Condicao,
    string TemplateMensagem,
    string? Recomendacao
);

/// <summary>
/// DTO de resposta de regra de alerta
/// </summary>
public record RegraAlertaDto(
    Guid Id,
    string Nome,
    string? Descricao,
    TipoAlerta TipoAlerta,
    string TipoAlertaNome,
    NivelSeveridade Severidade,
    string SeveridadeNome,
    bool Ativa,
    string Condicao,
    string TemplateMensagem,
    string? Recomendacao,
    DateTime DataCriacao,
    DateTime? DataAtualizacao
);

/// <summary>
/// DTO para estatísticas de alertas
/// </summary>
public record EstatisticasAlertasDto(
    int TotalAlertas,
    int AlertasAtivos,
    int AlertasVisualizados,
    int AlertasResolvidos,
    Dictionary<string, int> AlertasPorTipo,
    Dictionary<string, int> AlertasPorSeveridade
);
