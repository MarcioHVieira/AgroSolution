using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Validators;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Application.Validators;

public class RegistrarUsuarioDtoValidatorTests
{
    private readonly RegistrarUsuarioDtoValidator _validator;

    public RegistrarUsuarioDtoValidatorTests()
    {
        _validator = new RegistrarUsuarioDtoValidator();
    }

    [Fact]
    public void Validate_DevePassarComDadosValidos()
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            "11999999999",
            "12345678909"
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Ma")]
    public void Validate_DeveFalharComNomeInvalido(string nomeInvalido)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            nomeInvalido,
            "marcio@agrosolutions.com.br",
            "Senha@123",
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "NomeCompleto");
    }

    [Theory]
    [InlineData("")]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@agrosolutions.com.br")]
    public void Validate_DeveFalharComEmailInvalido(string emailInvalido)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            emailInvalido,
            "Senha@123",
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("senha")]
    [InlineData("SENHA123")]
    [InlineData("senha123")]
    [InlineData("Senha123")]
    [InlineData("Sen@1")]
    public void Validate_DeveFalharComSenhaInvalida(string senhaInvalida)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            senhaInvalida,
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }

    [Theory]
    [InlineData("119999999")]
    [InlineData("119999999999")]
    [InlineData("11abc999999")]
    public void Validate_DeveFalharComTelefoneInvalido(string telefoneInvalido)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            telefoneInvalido,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Telefone");
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012")]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("12345678901")]
    public void Validate_DeveFalharComCpfInvalido(string cpfInvalido)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            null,
            cpfInvalido
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Cpf");
    }

    [Theory]
    [InlineData("11999999999")]
    [InlineData("1199999999")]
    public void Validate_DevePassarComTelefoneValido(string telefoneValido)
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            telefoneValido,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DevePassarComCpfValido()
    {
        // Arrange - CPF válido: 12345678909
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            null,
            "12345678909"
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DevePassarComTelefoneECpfNulos()
    {
        // Arrange
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            "marcio@agrosolutions.com.br",
            "Senha@123",
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DeveFalharComNomeMuitoLongo()
    {
        // Arrange
        var nomeLongo = new string('a', 201);
        var dto = new RegistrarUsuarioDto(
            nomeLongo,
            "marcio@agrosolutions.com.br",
            "Senha@123",
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "NomeCompleto");
    }

    [Fact]
    public void Validate_DeveFalharComEmailMuitoLongo()
    {
        // Arrange
        var emailLongo = new string('a', 95) + "@t.com.br"; // Total > 100
        var dto = new RegistrarUsuarioDto(
            "Marcio Henrique",
            emailLongo,
            "Senha@123",
            null,
            null
        );

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}

public class ValidarCodigoDtoValidatorTests
{
    private readonly ValidarCodigoDtoValidator _validator;

    public ValidarCodigoDtoValidatorTests()
    {
        _validator = new ValidarCodigoDtoValidator();
    }

    [Fact]
    public void Validate_DevePassarComDadosValidos()
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("emailinvalido")]
    public void Validate_DeveFalharComEmailInvalido(string emailInvalido)
    {
        // Arrange
        var dto = new ValidarCodigoDto(emailInvalido, "123456");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void Validate_DeveFalharComCodigoInvalido(string codigoInvalido)
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", codigoInvalido);

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Codigo");
    }
}

public class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _validator;

    public LoginDtoValidatorTests()
    {
        _validator = new LoginDtoValidator();
    }

    [Fact]
    public void Validate_DevePassarComDadosValidos()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("emailinvalido")]
    public void Validate_DeveFalharComEmailInvalido(string emailInvalido)
    {
        // Arrange
        var dto = new LoginDto(emailInvalido, "Senha@123");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_DeveFalharComSenhaVazia()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "");

        // Act
        var resultado = _validator.Validate(dto);

        // Assert
        resultado.IsValid.Should().BeFalse();
        resultado.Errors.Should().Contain(e => e.PropertyName == "Senha");
    }
}
