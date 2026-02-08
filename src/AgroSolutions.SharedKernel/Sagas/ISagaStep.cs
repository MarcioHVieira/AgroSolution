namespace AgroSolutions.SharedKernel.Sagas;

/// <summary>
/// Interface base para um passo de Saga
/// </summary>
public interface ISagaStep<TData>
{
    /// <summary>
    /// Executa o passo da saga
    /// </summary>
    Task<SagaStepResult> ExecuteAsync(TData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compensa o passo da saga em caso de falha
    /// </summary>
    Task CompensateAsync(TData data, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado da execução de um passo
/// </summary>
public record SagaStepResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object>? Data { get; init; }

    public static SagaStepResult Ok(Dictionary<string, object>? data = null) =>
        new() { Success = true, Data = data };

    public static SagaStepResult Fail(string errorMessage) =>
        new() { Success = false, ErrorMessage = errorMessage };

}
