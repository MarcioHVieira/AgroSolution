using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Validators;
using AgroSolutions.IngestaoDados.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.IngestaoDados.Test.Application.Validators;

public class CriarSensorDtoValidatorTests
{
    private readonly CriarSensorDtoValidator _validator;

    public CriarSensorDtoValidatorTests()
    {
        _validator = new CriarSensorDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoPropriedadeIdVazio()
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.Empty,
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "PropriedadeId");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("SEN")]
    public void Validar_DeveFalhar_QuandoDeviceIdInvalido(string? deviceIdInvalido)
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            deviceIdInvalido!,
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DeviceId");
    }

    [Theory]
    [InlineData("sensor-001")]
    [InlineData("sensor@001")]
    public void Validar_DeveFalhar_QuandoDeviceIdComCaracteresInvalidos(string deviceIdInvalido)
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            deviceIdInvalido,
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DeviceId");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("AB")]
    public void Validar_DeveFalhar_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            nomeInvalido!,
            TipoSensor.Temperatura,
            15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1441)]
    public void Validar_DeveFalhar_QuandoIntervaloLeituraInvalido(int intervaloInvalido)
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            intervaloInvalido);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "IntervaloLeituraMinutos");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoLatitudeSemLongitude()
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15,
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
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15,
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
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15,
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

    [Theory]
    [InlineData(-501)]
    [InlineData(9001)]
    public void Validar_DeveFalhar_QuandoAltitudeForaDoIntervalo(decimal altitudeInvalida)
    {
        // Arrange
        var dto = new CriarSensorDto(
            Guid.NewGuid(),
            "SENSOR-001",
            "Sensor de Temperatura",
            TipoSensor.Temperatura,
            15,
            null,
            null,
            null,
            -23.5505m,
            -46.6333m,
            altitudeInvalida);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Altitude");
    }
}

public class AtualizarSensorDtoValidatorTests
{
    private readonly AtualizarSensorDtoValidator _validator;

    public AtualizarSensorDtoValidatorTests()
    {
        _validator = new AtualizarSensorDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new AtualizarSensorDto(
            "Sensor Atualizado",
            30,
            Guid.NewGuid(),
            "Novo Fabricante",
            "Novo Modelo",
            -22.9068m,
            -43.1729m,
            800m,
            "Novas observações");

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
        var dto = new AtualizarSensorDto(nomeInvalido!, 15);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Nome");
    }
}

public class RegistrarLeituraDtoValidatorTests
{
    private readonly RegistrarLeituraDtoValidator _validator;

    public RegistrarLeituraDtoValidatorTests()
    {
        _validator = new RegistrarLeituraDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(-5),
            85,
            -60,
            null);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("SEN")]
    public void Validar_DeveFalhar_QuandoDeviceIdInvalido(string? deviceIdInvalido)
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            deviceIdInvalido!,
            25.5m,
            "°C",
            DateTime.UtcNow);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "DeviceId");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    public void Validar_DeveFalhar_QuandoUnidadeInvalida(string? unidadeInvalida)
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            unidadeInvalida!,
            DateTime.UtcNow);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Unidade");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoTimestampMuitoAntigo()
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            "°C",
            DateTime.UtcNow.AddDays(-8));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "TimestampLeitura");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoTimestampMuitoFuturo()
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            "°C",
            DateTime.UtcNow.AddMinutes(10));

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "TimestampLeitura");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validar_DeveFalhar_QuandoNivelBateriaInvalido(int nivelInvalido)
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            "°C",
            DateTime.UtcNow,
            nivelInvalido);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "NivelBateria");
    }

    [Theory]
    [InlineData(-121)]
    [InlineData(1)]
    public void Validar_DeveFalhar_QuandoIntensidadeSinalInvalida(int intensidadeInvalida)
    {
        // Arrange
        var dto = new RegistrarLeituraDto(
            "SENSOR-001",
            25.5m,
            "°C",
            DateTime.UtcNow,
            null,
            intensidadeInvalida);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "IntensidadeSinal");
    }
}

public class RegistrarLeituraLoteDtoValidatorTests
{
    private readonly RegistrarLeituraLoteDtoValidator _validator;

    public RegistrarLeituraLoteDtoValidatorTests()
    {
        _validator = new RegistrarLeituraLoteDtoValidator();
    }

    [Fact]
    public void Validar_DevePassar_QuandoDadosValidos()
    {
        // Arrange
        var leituras = new List<RegistrarLeituraDto>
        {
            new RegistrarLeituraDto("SENSOR-001", 25.5m, "°C", DateTime.UtcNow.AddMinutes(-5)),
            new RegistrarLeituraDto("SENSOR-002", 60m, "%", DateTime.UtcNow.AddMinutes(-3))
        };
        var dto = new RegistrarLeituraLoteDto(leituras);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoListaVazia()
    {
        // Arrange
        var dto = new RegistrarLeituraLoteDto(new List<RegistrarLeituraDto>());

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Leituras");
    }

    [Fact]
    public void Validar_DeveFalhar_QuandoMaisDe1000Leituras()
    {
        // Arrange
        var leituras = Enumerable.Range(1, 1001)
            .Select(i => new RegistrarLeituraDto($"SENSOR-{i:D3}", 25.5m, "°C", DateTime.UtcNow))
            .ToList();
        var dto = new RegistrarLeituraLoteDto(leituras);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Leituras");
    }
}
