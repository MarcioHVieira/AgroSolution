using AgroSolutions.SharedKernel.Sagas;
using AgroSolutions.SharedKernel.Test.Mocks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.SharedKernel.Test.Sagas;

/// <summary>
/// Testes do SagaOrchestrator
/// CRÍTICO: Garante que transações distribuídas funcionam corretamente com rollback
/// </summary>
public class SagaOrchestratorTests
{
    private readonly Mock<ILogger<SagaOrchestrator<TestSagaData>>> _loggerMock;

    public SagaOrchestratorTests()
    {
        _loggerMock = new Mock<ILogger<SagaOrchestrator<TestSagaData>>>();
    }

    #region Testes de Sucesso

    [Fact]
    public async Task ExecuteAsync_ComTodosPassosBemSucedidos_DeveRetornarSucesso()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2");
        var step3 = new MockSagaStep<TestSagaData>("Passo3");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData { Nome = "Teste Saga" };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        step1.WasExecuted.Should().BeTrue();
        step2.WasExecuted.Should().BeTrue();
        step3.WasExecuted.Should().BeTrue();

        step1.WasCompensated.Should().BeFalse();
        step2.WasCompensated.Should().BeFalse();
        step3.WasCompensated.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_SemPassos_DeveRetornarSucesso()
    {
        // Arrange
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object);
        var data = new TestSagaData();

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ComUmPasso_DeveExecutarCorretamente()
    {
        // Arrange
        var step = new MockSagaStep<TestSagaData>("PassoUnico");
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step);

        var data = new TestSagaData { Nome = "Teste" };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeTrue();
        step.WasExecuted.Should().BeTrue();
        step.WasCompensated.Should().BeFalse();
    }

    #endregion

    #region Testes de Falha e Compensação

    [Fact]
    public async Task ExecuteAsync_QuandoSegundoPassoFalha_DeveCompensarPrimeiroPasso()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2") { ShouldFail = true };
        var step3 = new MockSagaStep<TestSagaData>("Passo3");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData { Nome = "Teste Compensação" };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Passo2");

        // Passo 1 foi executado e compensado
        step1.WasExecuted.Should().BeTrue();
        step1.WasCompensated.Should().BeTrue();

        // Passo 2 foi executado mas falhou
        step2.WasExecuted.Should().BeTrue();
        step2.WasCompensated.Should().BeFalse();

        // Passo 3 NÃO foi executado
        step3.WasExecuted.Should().BeFalse();
        step3.WasCompensated.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_QuandoTerceiroPassoFalha_DeveCompensarDoisPrimeirosPassos()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2");
        var step3 = new MockSagaStep<TestSagaData>("Passo3") { ShouldFail = true };

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData();

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeFalse();

        // Todos foram executados
        step1.WasExecuted.Should().BeTrue();
        step2.WasExecuted.Should().BeTrue();
        step3.WasExecuted.Should().BeTrue();

        // Passos 1 e 2 foram compensados (ordem inversa)
        step1.WasCompensated.Should().BeTrue();
        step2.WasCompensated.Should().BeTrue();
        step3.WasCompensated.Should().BeFalse(); // Não compensa passo que falhou
    }

    [Fact]
    public async Task ExecuteAsync_QuandoPrimeiroPassoFalha_NaoDeveExecutarProximos()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1") { ShouldFail = true };
        var step2 = new MockSagaStep<TestSagaData>("Passo2");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2);

        var data = new TestSagaData();

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeFalse();

        step1.WasExecuted.Should().BeTrue();
        step1.WasCompensated.Should().BeFalse(); // Não compensa passo que falhou

        step2.WasExecuted.Should().BeFalse(); // Não deve ser executado
        step2.WasCompensated.Should().BeFalse();
    }

    #endregion

    #region Testes de Exceções

    [Fact]
    public async Task ExecuteAsync_QuandoPassoLancaExcecao_DeveCompensarPassosAnteriores()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2")
        {
            ExceptionToThrow = new InvalidOperationException("Erro crítico no passo 2")
        };
        var step3 = new MockSagaStep<TestSagaData>("Passo3");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData();

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Erro crítico no passo 2");

        // Passo 1 compensado
        step1.WasExecuted.Should().BeTrue();
        step1.WasCompensated.Should().BeTrue();

        // Passo 3 não executado
        step3.WasExecuted.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_QuandoCancelado_DevePararExecucao()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2);

        var data = new TestSagaData();
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancela antes de executar

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await orchestrator.ExecuteAsync(data, cts.Token);
        });
    }

    #endregion

    #region Testes de Ordem de Compensação

    [Fact]
    public async Task ExecuteAsync_DeveCompensarNaOrdemInversa()
    {
        // Arrange
        var compensationOrder = new List<string>();

        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2");
        var step3 = new MockSagaStep<TestSagaData>("Passo3") { ShouldFail = true };

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2)
            .AddStep(step3);

        var data = new TestSagaData { Logs = compensationOrder };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeFalse();

        // Ordem de compensação: Passo2 ? Passo1 (inverso)
        step2.WasCompensated.Should().BeTrue();
        step1.WasCompensated.Should().BeTrue();
    }

    #endregion

    #region Testes com Dados

    [Fact]
    public async Task ExecuteAsync_DeveManterIntegridadeDosDados()
    {
        // Arrange
        var step1 = new MockSagaStep<TestSagaData>("Passo1");
        var step2 = new MockSagaStep<TestSagaData>("Passo2");

        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step1)
            .AddStep(step2);

        var data = new TestSagaData
        {
            Nome = "Teste Original",
            Contador = 42
        };

        // Act
        var result = await orchestrator.ExecuteAsync(data);

        // Assert
        result.Success.Should().BeTrue();

        // Dados mantidos
        data.Nome.Should().Be("Teste Original");
        data.Contador.Should().Be(42);
    }

    #endregion

    #region Testes de Log

    [Fact]
    public async Task ExecuteAsync_DeveLogarInicioEFimDaSaga()
    {
        // Arrange
        var step = new MockSagaStep<TestSagaData>("Passo1");
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step);

        var data = new TestSagaData();

        // Act
        await orchestrator.ExecuteAsync(data);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando execução da saga")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("executada com sucesso")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_QuandoFalha_DeveLogarErro()
    {
        // Arrange
        var step = new MockSagaStep<TestSagaData>("PassoComFalha") { ShouldFail = true };
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object)
            .AddStep(step);

        var data = new TestSagaData();

        // Act
        await orchestrator.ExecuteAsync(data);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("falhou")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Testes de Performance

    [Fact]
    public async Task ExecuteAsync_ComMuitosPassos_DeveExecutarRapidamente()
    {
        // Arrange
        var orchestrator = new SagaOrchestrator<TestSagaData>(_loggerMock.Object);

        for (int i = 0; i < 100; i++)
        {
            orchestrator.AddStep(new MockSagaStep<TestSagaData>($"Passo{i}"));
        }

        var data = new TestSagaData();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await orchestrator.ExecuteAsync(data);
        stopwatch.Stop();

        // Assert
        result.Success.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // < 1 segundo
    }

    #endregion
}
