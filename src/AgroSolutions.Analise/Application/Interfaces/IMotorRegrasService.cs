namespace AgroSolutions.Analise.Application.Interfaces;

/// <summary>
/// Interface do Motor de Regras de Alertas
/// </summary>
public interface IMotorRegrasService
{
    /// <summary>
    /// Processa uma nova leitura e avalia regras aplicáveis
    /// </summary>
    Task ProcessarLeituraEAvaliarRegrasAsync(LeituraParaAnaliseDto leitura);
}

/// <summary>
/// DTO com dados da leitura para análise
/// </summary>
public record LeituraParaAnaliseDto(
    Guid TalhaoId,
    int TipoSensor,
    decimal Valor,
    DateTime TimestampLeitura
);
