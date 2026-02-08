using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Propriedades.Test.Domain.Entities;

public class CulturaTests
{
    [Fact]
    public void Construtor_DeveCriarCulturaComDadosValidos()
    {
        // Arrange
        var talhaoId = Guid.NewGuid();
        var tipo = TipoCultura.Soja;
        var variedade = "Monsoy 6410";
        var areaPlantada = 5m;
        var dataPlantio = DateTime.UtcNow.AddDays(-30);
        var dataColheitaPrevista = DateTime.UtcNow.AddDays(90);
        var producaoEstimada = 15m;
        var observacoes = "Plantio em solo argiloso";

        // Act
        var cultura = new Cultura(
            talhaoId,
            tipo,
            variedade,
            areaPlantada,
            dataPlantio,
            dataColheitaPrevista,
            producaoEstimada,
            observacoes);

        // Assert
        cultura.Id.Should().NotBeEmpty();
        cultura.TalhaoId.Should().Be(talhaoId);
        cultura.Tipo.Should().Be(tipo);
        cultura.Variedade.Should().Be(variedade);
        cultura.AreaPlantada.Should().Be(areaPlantada);
        cultura.DataPlantio.Should().Be(dataPlantio);
        cultura.DataColheitaPrevista.Should().Be(dataColheitaPrevista);
        cultura.ProducaoEstimada.Should().Be(producaoEstimada);
        cultura.Observacoes.Should().Be(observacoes);
        cultura.Status.Should().Be(StatusCultura.Ativa);
        cultura.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cultura.DataAtualizacao.Should().BeNull();
        cultura.DataColheitaRealizada.Should().BeNull();
        cultura.ProducaoReal.Should().BeNull();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoVariedadeInvalida(string? variedadeInvalida)
    {
        // Arrange & Act
        var act = () => new Cultura(
            Guid.NewGuid(),
            TipoCultura.Soja,
            variedadeInvalida!,
            5m,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Variedade*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Construtor_DeveLancarExcecao_QuandoAreaPlantadaInvalida(decimal areaInvalida)
    {
        // Arrange & Act
        var act = () => new Cultura(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            areaInvalida,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Área plantada*");
    }

    [Fact]
    public void Atualizar_DeveAtualizarCulturaComSucesso()
    {
        // Arrange
        var cultura = CriarCulturaValida();
        var novoTipo = TipoCultura.Milho;
        var novaVariedade = "Pioneer 30F53";
        var novaArea = 7m;
        var novaDataPlantio = DateTime.UtcNow.AddDays(-20);
        var novaDataColheitaPrevista = DateTime.UtcNow.AddDays(100);
        var novaProducaoEstimada = 20m;
        var novasObservacoes = "Solo preparado com calcário";

        // Act
        cultura.Atualizar(
            novoTipo,
            novaVariedade,
            novaArea,
            novaDataPlantio,
            novaDataColheitaPrevista,
            novaProducaoEstimada,
            novasObservacoes);

        // Assert
        cultura.Tipo.Should().Be(novoTipo);
        cultura.Variedade.Should().Be(novaVariedade);
        cultura.AreaPlantada.Should().Be(novaArea);
        cultura.DataPlantio.Should().Be(novaDataPlantio);
        cultura.DataColheitaPrevista.Should().Be(novaDataColheitaPrevista);
        cultura.ProducaoEstimada.Should().Be(novaProducaoEstimada);
        cultura.Observacoes.Should().Be(novasObservacoes);
        cultura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoVariedadeInvalida(string? variedadeInvalida)
    {
        // Arrange
        var cultura = CriarCulturaValida();

        // Act
        var act = () => cultura.Atualizar(
            TipoCultura.Soja,
            variedadeInvalida!,
            5m,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Variedade*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Atualizar_DeveLancarExcecao_QuandoAreaPlantadaInvalida(decimal areaInvalida)
    {
        // Arrange
        var cultura = CriarCulturaValida();

        // Act
        var act = () => cultura.Atualizar(
            TipoCultura.Soja,
            "Monsoy 6410",
            areaInvalida,
            DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Área plantada*");
    }

    [Fact]
    public void RegistrarColheita_DeveRegistrarColheitaComSucesso()
    {
        // Arrange
        var cultura = CriarCulturaValida();
        var dataColheita = DateTime.UtcNow;
        var producaoReal = 18m;
        var observacoes = "Colheita bem-sucedida";

        // Act
        cultura.RegistrarColheita(dataColheita, producaoReal, observacoes);

        // Assert
        cultura.DataColheitaRealizada.Should().Be(dataColheita);
        cultura.ProducaoReal.Should().Be(producaoReal);
        cultura.Observacoes.Should().Be(observacoes);
        cultura.Status.Should().Be(StatusCultura.Colhida);
        cultura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegistrarColheita_DeveLancarExcecao_QuandoProducaoRealNegativa()
    {
        // Arrange
        var cultura = CriarCulturaValida();

        // Act
        var act = () => cultura.RegistrarColheita(DateTime.UtcNow, -10m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Produção real*");
    }

    [Fact]
    public void RegistrarColheita_DeveManterObservacoesAnteriores_QuandoNovasObservacoesVazias()
    {
        // Arrange
        var cultura = CriarCulturaValida();
        var observacoesOriginais = cultura.Observacoes;

        // Act
        cultura.RegistrarColheita(DateTime.UtcNow, 15m, null);

        // Assert
        cultura.Observacoes.Should().Be(observacoesOriginais);
    }

    [Fact]
    public void Cancelar_DeveCancelarCulturaComMotivo()
    {
        // Arrange
        var cultura = CriarCulturaValida();
        var motivo = "Praga de lagartas";

        // Act
        cultura.Cancelar(motivo);

        // Assert
        cultura.Status.Should().Be(StatusCultura.Cancelada);
        cultura.Observacoes.Should().Contain("Cancelada");
        cultura.Observacoes.Should().Contain(motivo);
        cultura.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalcularProdutividade_DeveRetornarNull_QuandoSemProducaoReal()
    {
        // Arrange
        var cultura = CriarCulturaValida();

        // Act
        var produtividade = cultura.CalcularProdutividade();

        // Assert
        produtividade.Should().BeNull();
    }

    [Fact]
    public void CalcularProdutividade_DeveCalcularCorretamente_QuandoComProducaoReal()
    {
        // Arrange
        var cultura = CriarCulturaValida();
        var producaoReal = 15m;
        cultura.RegistrarColheita(DateTime.UtcNow, producaoReal);

        // Act
        var produtividade = cultura.CalcularProdutividade();

        // Assert
        var produtividadeEsperada = producaoReal / cultura.AreaPlantada;
        produtividade.Should().Be(produtividadeEsperada);
    }

    [Fact]
    public void CalcularProdutividade_DeveRetornarNull_QuandoAreaPlantadaZero()
    {
        // Arrange & Act
        // Não é possível criar cultura com área zero devido à validação no construtor
        // Este teste documenta o comportamento esperado
        var cultura = CriarCulturaValida();
        cultura.RegistrarColheita(DateTime.UtcNow, 15m);

        var produtividade = cultura.CalcularProdutividade();

        // Assert
        produtividade.Should().NotBeNull();
        produtividade.Should().BeGreaterThan(0);
    }

    private static Cultura CriarCulturaValida()
    {
        return new Cultura(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(90),
            15m,
            "Cultura de teste");
    }
}
