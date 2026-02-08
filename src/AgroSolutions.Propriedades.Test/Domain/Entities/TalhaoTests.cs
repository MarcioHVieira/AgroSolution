using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Propriedades.Test.Domain.Entities;

public class TalhaoTests
{
    [Fact]
    public void Construtor_DeveCriarTalhaoComDadosValidos()
    {
        // Arrange
        var propriedadeId = Guid.NewGuid();
        var nome = "Talhão A1";
        var area = 10m;
        var descricao = "Talhão para cultivo de soja";
        var latitude = -23.5505m;
        var longitude = -46.6333m;
        var poligono = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[10,0],[10,10],[0,10],[0,0]]]}";

        // Act
        var talhao = new Talhao(propriedadeId, nome, area, descricao, latitude, longitude, poligono);

        // Assert
        talhao.Id.Should().NotBeEmpty();
        talhao.PropriedadeId.Should().Be(propriedadeId);
        talhao.Nome.Should().Be(nome);
        talhao.Descricao.Should().Be(descricao);
        talhao.Area.Should().Be(area);
        talhao.Latitude.Should().Be(latitude);
        talhao.Longitude.Should().Be(longitude);
        talhao.Poligono.Should().Be(poligono);
        talhao.Status.Should().Be(StatusTalhao.Disponivel);
        talhao.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        talhao.DataAtualizacao.Should().BeNull();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange & Act
        var act = () => new Talhao(Guid.NewGuid(), nomeInvalido!, 10m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Construtor_DeveLancarExcecao_QuandoAreaInvalida(decimal areaInvalida)
    {
        // Arrange & Act
        var act = () => new Talhao(Guid.NewGuid(), "Talhão A", areaInvalida);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Área*");
    }

    [Fact]
    public void Atualizar_DeveAtualizarTalhaoComSucesso()
    {
        // Arrange
        var talhao = CriarTalhaoValido();
        var novoNome = "Talhão B1";
        var novaArea = 15m;
        var novaDescricao = "Talhão para milho";
        var novaLatitude = -22.9068m;
        var novaLongitude = -43.1729m;
        var novoPoligono = "{\"type\":\"Polygon\",\"coordinates\":[[[0,0],[15,0],[15,15],[0,15],[0,0]]]}";

        // Act
        talhao.Atualizar(novoNome, novaArea, novaDescricao, novaLatitude, novaLongitude, novoPoligono);

        // Assert
        talhao.Nome.Should().Be(novoNome);
        talhao.Area.Should().Be(novaArea);
        talhao.Descricao.Should().Be(novaDescricao);
        talhao.Latitude.Should().Be(novaLatitude);
        talhao.Longitude.Should().Be(novaLongitude);
        talhao.Poligono.Should().Be(novoPoligono);
        talhao.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_DeveLancarExcecao_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var talhao = CriarTalhaoValido();

        // Act
        var act = () => talhao.Atualizar(nomeInvalido!, 10m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Atualizar_DeveLancarExcecao_QuandoAreaInvalida(decimal areaInvalida)
    {
        // Arrange
        var talhao = CriarTalhaoValido();

        // Act
        var act = () => talhao.Atualizar("Talhão A", areaInvalida);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Área*");
    }

    [Fact]
    public void MarcarComoEmUso_DeveAlterarStatusParaEmUso()
    {
        // Arrange
        var talhao = CriarTalhaoValido();

        // Act
        talhao.MarcarComoEmUso();

        // Assert
        talhao.Status.Should().Be(StatusTalhao.EmUso);
        talhao.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarcarComoDisponivel_DeveAlterarStatusParaDisponivel()
    {
        // Arrange
        var talhao = CriarTalhaoValido();
        talhao.MarcarComoEmUso();

        // Act
        talhao.MarcarComoDisponivel();

        // Assert
        talhao.Status.Should().Be(StatusTalhao.Disponivel);
        talhao.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarcarComoEmDescanso_DeveAlterarStatusParaEmDescanso()
    {
        // Arrange
        var talhao = CriarTalhaoValido();

        // Act
        talhao.MarcarComoEmDescanso();

        // Assert
        talhao.Status.Should().Be(StatusTalhao.EmDescanso);
        talhao.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PossuiCulturaAtiva_DeveRetornarFalse_QuandoNaoTemCulturasAtivas()
    {
        // Arrange
        var talhao = CriarTalhaoValido();

        // Act
        var possuiCulturaAtiva = talhao.PossuiCulturaAtiva();

        // Assert
        possuiCulturaAtiva.Should().BeFalse();
    }

    private static Talhao CriarTalhaoValido()
    {
        return new Talhao(Guid.NewGuid(), "Talhão A1", 10m, "Descrição teste");
    }
}
