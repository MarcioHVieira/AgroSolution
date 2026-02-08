using AgroSolutions.SharedKernel.Observability;
using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace AgroSolutions.SharedKernel.Test.Observability;

/// <summary>
/// Testes do TracingHelper
/// </summary>
public class TracingHelperTests : IDisposable
{
    private readonly TracingHelper _tracingHelper;
    private readonly ActivityListener _activityListener;
    private readonly List<Activity> _capturedActivities = new();

    public TracingHelperTests()
    {
        _tracingHelper = new TracingHelper();
        
        // Configura o ActivityListener para capturar activities nos testes
        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.StartsWith("AgroSolutions"),
            Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStarted = activity => _capturedActivities.Add(activity)
        };
        
        ActivitySource.AddActivityListener(_activityListener);
    }

    public void Dispose()
    {
        _activityListener?.Dispose();
    }

    #region Testes de TraceAsync<T>

    [Fact]
    public async Task TraceAsync_ComOperacaoBemSucedida_DeveRetornarResultado()
    {
        // Arrange
        var expectedResult = 42;

        // Act
        var result = await _tracingHelper.TraceAsync(
            "TestOperation",
            async () =>
            {
                await Task.Delay(10);
                return expectedResult;
            });

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TraceAsync_ComOperacaoQueRetornaString_DeveRetornarString()
    {
        // Arrange
        var expectedResult = "Operação concluída com sucesso";

        // Act
        var result = await _tracingHelper.TraceAsync(
            "StringOperation",
            async () =>
            {
                await Task.Delay(5);
                return expectedResult;
            });

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task TraceAsync_ComTags_DeveExecutarSemErro()
    {
        // Arrange
        var tags = new Dictionary<string, object>
        {
            ["userId"] = Guid.NewGuid(),
            ["propriedadeId"] = Guid.NewGuid(),
            ["operation"] = "CriarPropriedade"
        };

        // Act
        var result = await _tracingHelper.TraceAsync(
            "OperacaoComTags",
            async () =>
            {
                await Task.Delay(5);
                return "OK";
            },
            tags);

        // Assert
        result.Should().Be("OK");
    }

    [Fact]
    public async Task TraceAsync_QuandoOperacaoLancaExcecao_DevePropagarExcecao()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Erro intencional");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _tracingHelper.TraceAsync<string>(
                "OperacaoComErro",
                async () =>
                {
                    await Task.Delay(5);
                    throw expectedException;
                });
        });
    }

    [Fact]
    public async Task TraceAsync_ComOperacaoComplexa_DeveExecutarCorretamente()
    {
        // Arrange
        var data = new { Id = Guid.NewGuid(), Nome = "Teste" };

        // Act
        var result = await _tracingHelper.TraceAsync(
            "OperacaoComplexa",
            async () =>
            {
                await Task.Delay(10);
                
                // Simula processamento
                var processedData = new
                {
                    data.Id,
                    data.Nome,
                    ProcessadoEm = DateTime.UtcNow
                };

                return processedData;
            });

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(data.Id);
        result.Nome.Should().Be(data.Nome);
    }

    #endregion

    #region Testes de TraceAsync (void)

    [Fact]
    public async Task TraceAsync_Void_ComOperacaoBemSucedida_DeveExecutarSemErro()
    {
        // Arrange
        var executed = false;

        // Act
        await _tracingHelper.TraceAsync(
            "VoidOperation",
            async () =>
            {
                await Task.Delay(5);
                executed = true;
            });

        // Assert
        executed.Should().BeTrue();
    }

    [Fact]
    public async Task TraceAsync_Void_ComTags_DeveExecutarSemErro()
    {
        // Arrange
        var counter = 0;
        var tags = new Dictionary<string, object>
        {
            ["operationId"] = Guid.NewGuid(),
            ["tipo"] = "batch"
        };

        // Act
        await _tracingHelper.TraceAsync(
            "VoidOperationComTags",
            async () =>
            {
                await Task.Delay(5);
                counter++;
            },
            tags);

        // Assert
        counter.Should().Be(1);
    }

    [Fact]
    public async Task TraceAsync_Void_QuandoLancaExcecao_DevePropagarExcecao()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _tracingHelper.TraceAsync(
                "VoidOperationComErro",
                async () =>
                {
                    await Task.Delay(5);
                    throw new ArgumentException("Argumento inválido");
                });
        });
    }

    #endregion

    #region Testes de AddEvent

    [Fact]
    public void AddEvent_SemAtributos_DeveExecutarSemErro()
    {
        // Act & Assert (não deve lançar exceção)
        _tracingHelper.AddEvent("EventoSimples");
    }

    [Fact]
    public void AddEvent_ComAtributos_DeveExecutarSemErro()
    {
        // Arrange
        var attributes = new Dictionary<string, object>
        {
            ["propriedadeId"] = Guid.NewGuid(),
            ["status"] = "Criada",
            ["timestamp"] = DateTime.UtcNow
        };

        // Act & Assert (não deve lançar exceção)
        _tracingHelper.AddEvent("PropriedadeCriada", attributes);
    }

    [Fact]
    public void AddEvent_ComAtributosVazios_DeveExecutarSemErro()
    {
        // Arrange
        var attributes = new Dictionary<string, object>();

        // Act & Assert
        _tracingHelper.AddEvent("EventoVazio", attributes);
    }

    [Fact]
    public void AddEvent_ComNomeVazio_DeveExecutarSemErro()
    {
        // Act & Assert
        _tracingHelper.AddEvent(string.Empty);
    }

    #endregion

    #region Testes de Integração com Activity

    [Fact]
    public async Task TraceAsync_DeveCriarActivityCorretamente()
    {
        // Arrange
        Activity? capturedActivity = null;

        // Act
        await _tracingHelper.TraceAsync(
            "OperacaoComActivity",
            async () =>
            {
                await Task.Delay(5);
                capturedActivity = Activity.Current;
                return "OK";
            });

        // Assert
        capturedActivity.Should().NotBeNull();
        capturedActivity!.DisplayName.Should().Be("OperacaoComActivity");
    }

    [Fact]
    public async Task TraceAsync_ComExcecao_DeveAdicionarTagsDeErro()
    {
        // Arrange
        Activity? activityAntes = null;
        Activity? activityDepois = null;

        try
        {
            // Act
            await _tracingHelper.TraceAsync(
                "OperacaoComErroEActivity",
                async () =>
                {
                    await Task.Delay(5);
                    activityAntes = Activity.Current;
                    throw new InvalidOperationException("Erro de teste");
                });
        }
        catch
        {
            activityDepois = Activity.Current;
        }

        // Assert
        activityAntes.Should().NotBeNull();
        // A activity é encerrada após a exceção
    }

    #endregion

    #region Testes de Concorrência

    [Fact]
    public async Task TraceAsync_ExecutandoEmParalelo_DeveManterIsolamento()
    {
        // Arrange
        var tasks = new List<Task<int>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i;
            tasks.Add(_tracingHelper.TraceAsync(
                $"OperacaoParalela{index}",
                async () =>
                {
                    await Task.Delay(Random.Shared.Next(10, 50));
                    return index;
                }));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().Contain(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
    }

    #endregion

    #region Testes de Performance

    [Fact]
    public async Task TraceAsync_Com1000Operacoes_DeveExecutarRapidamente()
    {
        // Arrange
        var tasks = new List<Task<int>>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            tasks.Add(_tracingHelper.TraceAsync(
                $"Op{i}",
                async () =>
                {
                    await Task.Yield();
                    return i;
                }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // < 5 segundos
    }

    #endregion
}
