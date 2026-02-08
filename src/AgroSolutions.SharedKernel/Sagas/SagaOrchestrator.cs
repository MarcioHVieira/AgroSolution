using Microsoft.Extensions.Logging;

namespace AgroSolutions.SharedKernel.Sagas;

/// <summary>
/// Orquestrador de Saga (padrão Orquestração)
/// </summary>
public class SagaOrchestrator<TData>
{
    private readonly List<ISagaStep<TData>> _steps = new();
    private readonly List<ISagaStep<TData>> _executedSteps = new();
    private readonly ILogger<SagaOrchestrator<TData>> _logger;

    public SagaOrchestrator(ILogger<SagaOrchestrator<TData>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adiciona um passo à saga
    /// </summary>
    public SagaOrchestrator<TData> AddStep(ISagaStep<TData> step)
    {
        _steps.Add(step);
        return this;
    }

    /// <summary>
    /// Executa a saga com compensação automática em caso de falha
    /// </summary>
    public async Task<SagaExecutionResult> ExecuteAsync(TData data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando execução da saga com {StepCount} passos", _steps.Count);

        try
        {
            foreach (var step in _steps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var stepName = step.GetType().Name;
                _logger.LogInformation("Executando passo: {StepName}", stepName);

                var result = await step.ExecuteAsync(data, cancellationToken);

                if (result.Success)
                {
                    _executedSteps.Add(step);
                    _logger.LogInformation("Passo {StepName} executado com sucesso", stepName);
                }
                else
                {
                    _logger.LogError("Passo {StepName} falhou: {ErrorMessage}", stepName, result.ErrorMessage);
                    await CompensateAsync(data, cancellationToken);
                    return SagaExecutionResult.Fail($"Falha no passo {stepName}: {result.ErrorMessage}");
                }
            }

            _logger.LogInformation("Saga executada com sucesso. Total de passos: {Count}", _executedSteps.Count);
            return SagaExecutionResult.Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Saga cancelada pelo usuário");
            await CompensateAsync(data, CancellationToken.None); // Usar CancellationToken.None na compensação
            throw; // Propaga a exceção de cancelamento
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante execução da saga: {Message}", ex.Message);
            await CompensateAsync(data, cancellationToken);
            return SagaExecutionResult.Fail($"Exceção durante saga: {ex.Message}");
        }
    }

    /// <summary>
    /// Compensa todos os passos executados (rollback)
    /// </summary>
    private async Task CompensateAsync(TData data, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Iniciando compensação de {Count} passos executados", _executedSteps.Count);

        // Compensar na ordem inversa
        _executedSteps.Reverse();

        foreach (var step in _executedSteps)
        {
            var stepName = step.GetType().Name;

            try
            {
                _logger.LogInformation("Compensando passo: {StepName}", stepName);
                await step.CompensateAsync(data, cancellationToken);
                _logger.LogInformation("Passo {StepName} compensado com sucesso", stepName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compensar passo {StepName}: {Message}", stepName, ex.Message);
                // Continua compensando os outros passos
            }
        }

        _logger.LogInformation("Compensação concluída");
    }
}

/// <summary>
/// Resultado da execução da saga
/// </summary>
public record SagaExecutionResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static SagaExecutionResult Ok() => new() { Success = true };
    public static SagaExecutionResult Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}

