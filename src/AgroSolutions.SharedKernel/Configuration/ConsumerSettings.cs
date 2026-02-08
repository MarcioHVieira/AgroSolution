namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configurações centralizadas para RabbitMQ Consumers
/// Evita magic numbers e facilita ajustes por ambiente
/// </summary>
public class ConsumerSettings
{
    /// <summary>
    /// Tempo de espera (em segundos) antes de iniciar o consumer
    /// Aguarda RabbitMQ e banco de dados estarem prontos
    /// </summary>
    public int StartupDelaySeconds { get; set; } = 10;

    /// <summary>
    /// Intervalo (em segundos) para reconexão automática do RabbitMQ
    /// </summary>
    public int NetworkRecoveryIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Número máximo de tentativas de reprocessamento antes de enviar para DLQ
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Tempo de vida (em milissegundos) de uma mensagem na fila
    /// Após esse tempo, a mensagem é movida para DLQ
    /// </summary>
    public int MessageTtlMilliseconds { get; set; } = 300000; // 5 minutos

    /// <summary>
    /// Base para cálculo do delay exponencial entre retries (em segundos)
    /// Delay = RetryDelayBaseSeconds * Math.Pow(RetryBackoffMultiplier, tentativa)
    /// </summary>
    public int RetryDelayBaseSeconds { get; set; } = 5;

    /// <summary>
    /// Multiplicador para backoff exponencial entre retries
    /// </summary>
    public int RetryBackoffMultiplier { get; set; } = 3;
}
