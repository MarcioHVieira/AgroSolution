using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Validators;
using AgroSolutions.Propriedades.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Propriedades.Test.Application.Validators;

public class CriarPropriedadeDtoValidatorTests
{
    private readonly CriarPropriedadeDtoValidator _validator;

    public CriarPropriedadeDtoValidatorTests()
    {
        _validator = new CriarPropriedadeDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Boa Vista",
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("AB")]
    public void Validar_DeveFalhar_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            nomeInvalido!,
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(1000001)]
    public void Validar_DeveFalhar_QuandoAreaTotalInvalida(decimal areaInvalida)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            areaInvalida,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "AreaTotal");
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("1234-567")]
    public void Validar_DeveFalhar_QuandoCepInvalido(string cepInvalido)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            cepInvalido,
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Cep");
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("SE1")]
    [InlineData("se")]
    public void Validar_DeveFalhar_QuandoEstadoInvalido(string estadoInvalido)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            estadoInvalido);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Estado");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoLatitudeSemLongitude()
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE",
            null,
            null,
            null,
            -23.5505m,
            null);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Validar_DeveFalhar_QuandoLatitudeForaDoIntervalo(decimal latitudeInvalida)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE",
            null,
            null,
            null,
            latitudeInvalida,
            -46.6333m);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Latitude");
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Validar_DeveFalhar_QuandoLongitudeForaDoIntervalo(decimal longitudeInvalida)
    {
        // Arrange
        var dto = new CriarPropriedadeDto(
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE",
            null,
            null,
            null,
            -23.5505m,
            longitudeInvalida);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Longitude");
    }
}

public class AtualizarPropriedadeDtoValidatorTests
{
    private readonly AtualizarPropriedadeDtoValidator _validator;

    public AtualizarPropriedadeDtoValidatorTests()
    {
        _validator = new AtualizarPropriedadeDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new AtualizarPropriedadeDto(
            "Fazenda Nova Vista",
            150m,
            TipoPropriedade.Sitio,
            "Nova descrição",
            -23.5505m,
            -46.6333m);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("AB")]
    public void Validar_DeveFalhar_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var dto = new AtualizarPropriedadeDto(
            nomeInvalido!,
            150m,
            TipoPropriedade.Sitio);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }
}

public class CriarTalhaoDtoValidatorTests
{
    private readonly CriarTalhaoDtoValidator _validator;

    public CriarTalhaoDtoValidatorTests()
    {
        _validator = new CriarTalhaoDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new CriarTalhaoDto(
            Guid.NewGuid(),
            "Talhão A1",
            10m,
            "Descrição teste");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("AB")]
    public void Validar_DeveFalhar_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var dto = new CriarTalhaoDto(
            Guid.NewGuid(),
            nomeInvalido!,
            10m);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(50001)]
    public void Validar_DeveFalhar_QuandoAreaInvalida(decimal areaInvalida)
    {
        // Arrange
        var dto = new CriarTalhaoDto(
            Guid.NewGuid(),
            "Talhão A1",
            areaInvalida);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Area");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoPropriedadeIdVazio()
    {
        // Arrange
        var dto = new CriarTalhaoDto(
            Guid.Empty,
            "Talhão A1",
            10m);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PropriedadeId");
    }
}

public class CriarCulturaDtoValidatorTests
{
    private readonly CriarCulturaDtoValidator _validator;

    public CriarCulturaDtoValidatorTests()
    {
        _validator = new CriarCulturaDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new CriarCulturaDto(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(90),
            15m,
            "Cultura teste");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("A")]
    public void Validar_DeveFalhar_QuandoVariedadeInvalida(string? variedadeInvalida)
    {
        // Arrange
        var dto = new CriarCulturaDto(
            Guid.NewGuid(),
            TipoCultura.Soja,
            variedadeInvalida!,
            5m,
            DateTime.UtcNow.AddDays(-30));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Variedade");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(50001)]
    public void Validar_DeveFalhar_QuandoAreaPlantadaInvalida(decimal areaInvalida)
    {
        // Arrange
        var dto = new CriarCulturaDto(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            areaInvalida,
            DateTime.UtcNow.AddDays(-30));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "AreaPlantada");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoDataPlantioFutura()
    {
        // Arrange
        var dto = new CriarCulturaDto(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(10));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DataPlantio");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoDataColheitaAnteriorAoPlantio()
    {
        // Arrange
        var dataPlantio = DateTime.UtcNow.AddDays(-30);
        var dto = new CriarCulturaDto(
            Guid.NewGuid(),
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            dataPlantio,
            dataPlantio.AddDays(-10)); // Data de colheita anterior ao plantio

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DataColheitaPrevista");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoTalhaoIdVazio()
    {
        // Arrange
        var dto = new CriarCulturaDto(
            Guid.Empty,
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(-30));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "TalhaoId");
    }
}

public class RegistrarColheitaDtoValidatorTests
{
    private readonly RegistrarColheitaDtoValidator _validator;

    public RegistrarColheitaDtoValidatorTests()
    {
        _validator = new RegistrarColheitaDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new RegistrarColheitaDto(
            DateTime.UtcNow.AddDays(-1), // Usar data no passado para evitar problemas de timing
            18m,
            "Colheita realizada com sucesso");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoDataColheitaFutura()
    {
        // Arrange
        var dto = new RegistrarColheitaDto(
            DateTime.UtcNow.AddDays(10),
            18m);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DataColheita");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(1000001)]
    public void Validar_DeveFalhar_QuandoProducaoRealInvalida(decimal producaoInvalida)
    {
        // Arrange
        var dto = new RegistrarColheitaDto(
            DateTime.UtcNow,
            producaoInvalida);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "ProducaoReal");
    }
}
