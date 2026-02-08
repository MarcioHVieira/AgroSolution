namespace AgroSolutions.Notificacoes.Domain.Enums;

/// <summary>
/// Tipos de notificação
/// </summary>
public enum TipoNotificacao
{
    /// <summary>
    /// Notificação por e-mail
    /// </summary>
    Email = 1,

    /// <summary>
    /// Notificação por SMS (futuro)
    /// </summary>
    SMS = 2,

    /// <summary>
    /// Notificação push (futuro)
    /// </summary>
    Push = 3,

    /// <summary>
    /// Notificação no sistema (futuro)
    /// </summary>
    InApp = 4
}

/// <summary>
/// Status da notificação
/// </summary>
public enum StatusNotificacao
{
    /// <summary>
    /// Aguardando envio
    /// </summary>
    Pendente = 1,

    /// <summary>
    /// Enviada com sucesso
    /// </summary>
    Enviada = 2,

    /// <summary>
    /// Falha no envio
    /// </summary>
    Falha = 3,

    /// <summary>
    /// Tentando reenviar
    /// </summary>
    Reenviando = 4
}

/// <summary>
/// Prioridade da notificação
/// </summary>
public enum PrioridadeNotificacao
{
    /// <summary>
    /// Baixa prioridade
    /// </summary>
    Baixa = 1,

    /// <summary>
    /// Normal
    /// </summary>
    Normal = 2,

    /// <summary>
    /// Alta prioridade
    /// </summary>
    Alta = 3,

    /// <summary>
    /// Urgente
    /// </summary>
    Urgente = 4
}

/// <summary>
/// Tipos de alerta que podem ser gerados (espelho de AgroSolutions.Analise)
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
/// Nível de severidade do alerta (espelho de AgroSolutions.Analise)
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
