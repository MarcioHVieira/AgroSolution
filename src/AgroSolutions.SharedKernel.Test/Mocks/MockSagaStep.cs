using AgroSolutions.SharedKernel.Sagas;

namespace AgroSolutions.SharedKernel.Test.Mocks;

/// <summary>
/// Mock simples de ISagaStep para testes
/// </summary>
public class MockSagaStep<TData> : ISagaStep<TData>
{
    public string StepName { get; }
    public bool ShouldFail { get; set; }
    public bool WasExecuted { get; private set; }
    public bool WasCompensated { get; private set; }
    public Exception? ExceptionToThrow { get; set; }

    public MockSagaStep(string stepName)
    {
        StepName = stepName;
    }

    public Task<SagaStepResult> ExecuteAsync(TData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        WasExecuted = true;

        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        if (ShouldFail)
            return Task.FromResult(SagaStepResult.Fail($"Falha intencional em {StepName}"));

        return Task.FromResult(SagaStepResult.Ok(new Dictionary<string, object>
        {
            ["StepName"] = StepName,
            ["ExecutedAt"] = DateTime.UtcNow
        }));
    }

    public Task CompensateAsync(TData data, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        WasCompensated = true;
        return Task.CompletedTask;
    }

    public void Reset()
    {
        WasExecuted = false;
        WasCompensated = false;
        ShouldFail = false;
        ExceptionToThrow = null;
    }
}

/// <summary>
/// DTO de teste para as sagas
/// </summary>
public class TestSagaData
{
    public string Nome { get; set; } = string.Empty;
    public int Contador { get; set; }
    public List<string> Logs { get; set; } = new();
}
