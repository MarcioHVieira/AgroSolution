namespace AgroSolutions.Analise.Domain.Enums;

/// <summary>
/// Tipos de alerta que podem ser gerados
/// </summary>
public enum TipoAlerta
{
    /// <summary>
    /// Alerta de seca - umidade do solo baixa
    /// </summary>
    Seca = 1,

    /// <summary>
    /// Alerta de geada - temperatura muito baixa
    /// </summary>
    Geada = 2,

    /// <summary>
    /// Alerta de calor excessivo
    /// </summary>
    CalorExcessivo = 3,

    /// <summary>
    /// Alerta de excesso de umidade
    /// </summary>
    ExcessoUmidade = 4,

    /// <summary>
    /// Risco de praga por condições climáticas
    /// </summary>
    RiscoPraga = 5,

    /// <summary>
    /// Condições ideais de irrigação
    /// </summary>
    IrrigacaoRecomendada = 6
}

/// <summary>
/// Nível de severidade do alerta
/// </summary>
public enum NivelSeveridade
{
    /// <summary>
    /// Informativo - situação normal
    /// </summary>
    Informativo = 0,

    /// <summary>
    /// Baixo - atenção recomendada
    /// </summary>
    Baixo = 1,

    /// <summary>
    /// Médio - ação recomendada
    /// </summary>
    Medio = 2,

    /// <summary>
    /// Alto - ação necessária
    /// </summary>
    Alto = 3,

    /// <summary>
    /// Crítico - ação imediata necessária
    /// </summary>
    Critico = 4
}

/// <summary>
/// Status do alerta
/// </summary>
public enum StatusAlerta
{
    /// <summary>
    /// Alerta ativo
    /// </summary>
    Ativo = 1,

    /// <summary>
    /// Alerta visualizado pelo produtor
    /// </summary>
    Visualizado = 2,

    /// <summary>
    /// Alerta em andamento
    /// </summary>
    EmAndamento = 3,

    /// <summary>
    /// Alerta resolvido
    /// </summary>
    Resolvido = 4,

    /// <summary>
    /// Alerta ignorado
    /// </summary>
    Ignorado = 5
}
