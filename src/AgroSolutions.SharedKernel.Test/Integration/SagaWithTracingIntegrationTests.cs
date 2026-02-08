using AgroSolutions.SharedKernel.Observability;
using AgroSolutions.SharedKernel.Sagas;
using AgroSolutions.SharedKernel.Test.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace AgroSolutions.SharedKernel.Test.Integration;

/// <summary>
/// Testes de integração entre TracingHelper e SagaOrchestrator
/// Simula cenários reais de uso
/// </summary>
public class SagaWithTracingIntegrationTests : IDisposable
{
    private readonly TracingHelper _tracing;
    private readonly Mock<ILogger<SagaOrchestrator<TestSagaData>>> _loggerMock;
    private readonly ActivityListener _activityListener;

    public SagaWithTracingIntegrationTests()
    {
        _tracing = new TracingHelper();
        _loggerMock = new Mock<ILogger<SagaOrchestrator<TestSagaData>>>();
        
        // Configura o ActivityListener para capturar activities nos testes
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith("AgroSolutions"),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded
        };
        
        ActivitySource.AddActivityListener(_activityListener);
    }

    public void Dispose()
    {
        _activityListener?.Dispose();
    }

    [Fact]
    public async Task SagaComTracing_CenarioCompleto_DeveFuncionarCorretamente()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("CriarPropriedade");
        var step2 = new MockSagaStep<TestSagaData>("CriarTalhoes");
        var step3 = new MockSagaStep<TestSagaData>("PublicarEvento");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData
        {
            Nome = "Fazenda São João",
            Contador = 0
        };

        // Act - Executa saga dentro de um trace
        var result = await _tracing.TraceAsync(
            "CriarPropriedadeCompleta",
            async () =>
            {
                Activity.Current?.AddTag("propriedade.nome", data.Nome);
                
                var sagaResult = await orchestrator.ExecuteAsync(data);
                
                if (sagaResult.Success)
                {
                    _tracing.AddEvent("PropriedadeCriadaComSucesso", new Dictionary<string, object>
                    {
                        ["propriedade"] = data.Nome,
                        ["passos_executados"] = 3
                    });
                }

                return sagaResult;
            },
            new Dictionary<string, object>
            {
                ["saga.type"] = "CriarPropriedadeCompleta",
                ["saga.steps"] = 3
            });

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        step1.WasExecuted.Should().BeTrue();
        step2.WasExecuted.Should().BeTrue();
        step3.WasExecuted.Should().BeTrue();

        step1.WasCompensated.Should().BeFalse();
        step2.WasCompensated.Should().BeFalse();
        step3.WasCompensated.Should().BeFalse();
    }

    [Fact]
    public async Task SagaComTracing_QuandoFalha_DeveRegistrarErroNoTrace()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("CriarPropriedade");
        var step2 = new MockSagaStep<TestSagaData>("CriarTalhoes") { ShouldFail = true };
        var step3 = new MockSagaStep<TestSagaData>("PublicarEvento");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData { Nome = "Fazenda com Erro" };

        Activity? capturedActivity = null;

        // Act
        var result = await _tracing.TraceAsync(
            "SagaComFalha",
            async () =>
            {
                capturedActivity = Activity.Current;
                
                var sagaResult = await orchestrator.ExecuteAsync(data);

                if (!sagaResult.Success)
                {
                    _tracing.AddEvent("SagaFalhou", new Dictionary<string, object>
                    {
                        ["erro"] = sagaResult.ErrorMessage ?? "Desconhecido"
                    });
                }

                return sagaResult;
            });

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("CriarTalhoes");

        // Compensação executada
        step1.WasCompensated.Should().BeTrue();
        
        capturedActivity.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipIasSagasComTracing_ExecutandoEmParalelo_DeveFuncionar()
    {
        // Arrange
        var tasks = new List<Task<SagaExecutionResult>>();

        // Act - Executa 5 sagas em paralelo
        for (int i = 0; i < 5; i++)
        {
            var sagaIndex = i;
            
            tasks.Add(_tracing.TraceAsync(
                $"Saga{sagaIndex}",
                async () =>
                {
                    var step1 = new MockSagaStep<TestSagaData>($"Passo1_Saga{sagaIndex}");
                    var step2 = new MockSagaStep<TestSagaData>($"Passo2_Saga{sagaIndex}");

                    var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
                        .AddStep(step1)
                        .AddStep(step2);

                    var data = new TestSagaData { Nome = $"Saga{sagaIndex}" };

                    await Task.Delay(Random.Shared.Next(10, 50));

                    return await orchestrator.ExecuteAsync(data);
                },
                new Dictionary<string, object>
                {
                    ["saga.index"] = sagaIndex
                }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().OnlyContain(r => r.Success);
    }

    [Fact]
    public async Task SagaComTracingAninhado_DeveMantearHierarquia()
    {
        // Arrange
        var step = new MockSagaStep<TestSagaData>("PassoComSubOperacoes");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step);

        var data = new TestSagaData { Nome = "Teste Aninhado" };

        // Act - Trace pai ? Saga ? Trace filho
        var result = await _tracing.TraceAsync(
            "OperacaoPrincipal",
            async () =>
            {
                _tracing.AddEvent("IniciandoSaga");

                var sagaResult = await _tracing.TraceAsync(
                    "ExecutarSaga",
                    async () => await orchestrator.ExecuteAsync(data));

                _tracing.AddEvent("SagaConcluida");

                return sagaResult;
            });

        // Assert
        result.Success.Should().BeTrue();
        step.WasExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task SagaLonga_Com10Passos_DeveFuncionarComTracing()
    {
        // Arrange
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object);

        for (int i = 1; i <= 10; i++)
        {
            orchestrator.AddStep(new MockSagaStep<TestSagaData>($"Passo{i}"));
        }

        var data = new TestSagaData { Nome = "Saga Longa" };

        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var result = await _tracing.TraceAsync(
            "SagaLonga",
            async () =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    _tracing.AddEvent($"ExecutandoPasso{i}");
                }

                return await orchestrator.ExecuteAsync(data);
            },
            new Dictionary<string, object>
            {
                ["saga.total_steps"] = 10
            });

        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 segundo
    }
}
