using AgroSolutions.SharedKernel.Sagas;
using FluentAssertions;

namespace AgroSolutions.SharedKernel.Test.Sagas;

/// <summary>
/// Testes para SagaStepResult
/// </summary>
public class SagaStepResultTests
{
    [Fact]
    public void Ok_SemDados_DeveCriarResultadoComSucesso()
    {
        // Act
        var result = SagaStepResult.Ok();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Ok_ComDados_DeveCriarResultadoComSucessoEDados()
    {
        // Arrange
        var data = new Dictionary<string, object>
        {
            ["PropriedadeId"] = Guid.NewGuid(),
            ["Nome"] = "Fazenda Teste"
        };

        // Act
        var result = SagaStepResult.Ok(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().ContainKey("PropriedadeId");
        result.Data.Should().ContainKey("Nome");
        result.Data!["Nome"].Should().Be("Fazenda Teste");
    }

    [Fact]
    public void Fail_ComMensagemDeErro_DeveCriarResultadoComFalha()
    {
        // Arrange
        var errorMessage = "Erro ao criar propriedade";

        // Act
        var result = SagaStepResult.Fail(errorMessage);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Fail_ComMensagemVazia_DeveAceitarMensagemVazia()
    {
        // Act
        var result = SagaStepResult.Fail(string.Empty);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(string.Empty);
    }
}

/// <summary>
/// Testes para SagaExecutionResult
/// </summary>
public class SagaExecutionResultTests
{
    [Fact]
    public void Ok_DeveCriarResultadoComSucesso()
    {
        // Act
        var result = SagaExecutionResult.Ok();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Fail_ComMensagem_DeveCriarResultadoComFalha()
    {
        // Arrange
        var errorMessage = "Falha na saga: Passo 3 não pôde ser executado";

        // Act
        var result = SagaExecutionResult.Fail(errorMessage);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void Fail_ComMensagemDetalhada_DevePreservarMensagemCompleta()
    {
        // Arrange
        var errorMessage = "Falha no passo CriarPropriedadeStep: Erro ao validar CEP 00000-000. " +
                          "Motivo: CEP inválido ou não encontrado.";

        // Act
        var result = SagaExecutionResult.Fail(errorMessage);

        // Assert
        result.ErrorMessage.Should().Contain("CriarPropriedadeStep");
        result.ErrorMessage.Should().Contain("CEP inválido");
    }
}
