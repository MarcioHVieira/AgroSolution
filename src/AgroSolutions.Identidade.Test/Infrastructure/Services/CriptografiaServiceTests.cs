using AgroSolutions.Identidade.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AgroSolutions.Identidade.Test.Infrastructure.Services;

public class CriptografiaServiceTests
{
    private readonly CriptografiaService _criptografiaService;

    public CriptografiaServiceTests()
    {
        _criptografiaService = new CriptografiaService();
    }

    [Fact]
    public void GerarHash_DeveGerarHashValido()
    {
        // Arrange
        var senha = "SenhaForte@123";

        // Act
        var hash = _criptografiaService.GerarHash(senha);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$argon2id$");
        hash.Should().Contain("$v=19$");
    }

    [Fact]
    public void GerarHash_DeveGerarHashesDiferentesParaMesmaSenha()
    {
        // Arrange
        var senha = "SenhaForte@123";

        // Act
        var hash1 = _criptografiaService.GerarHash(senha);
        var hash2 = _criptografiaService.GerarHash(senha);

        // Assert
        hash1.Should().NotBe(hash2); // Diferentes por causa do salt aleatório
    }

    [Fact]
    public void VerificarSenha_DeveRetornarTrueParaSenhaCorreta()
    {
        // Arrange
        var senha = "SenhaForte@123";
        var hash = _criptografiaService.GerarHash(senha);

        // Act
        var resultado = _criptografiaService.VerificarSenha(senha, hash);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void VerificarSenha_DeveRetornarFalseParaSenhaIncorreta()
    {
        // Arrange
        var senha = "SenhaForte@123";
        var senhaIncorreta = "SenhaErrada@456";
        var hash = _criptografiaService.GerarHash(senha);

        // Act
        var resultado = _criptografiaService.VerificarSenha(senhaIncorreta, hash);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void VerificarSenha_DeveRetornarFalseParaHashInvalido()
    {
        // Arrange
        var senha = "SenhaForte@123";
        var hashInvalido = "hashinvalido";

        // Act
        var resultado = _criptografiaService.VerificarSenha(senha, hashInvalido);

        // Assert
        resultado.Should().BeFalse();
    }

    [Theory]
    [InlineData("senha123")]
    [InlineData("Password@2024")]
    [InlineData("Teste#Seguro!456")]
    [InlineData("a")]
    [InlineData("1234567890123456789012345678901234567890")]
    public void VerificarSenha_DeveValidarDiferentesSenhas(string senha)
    {
        // Arrange
        var hash = _criptografiaService.GerarHash(senha);

        // Act
        var resultado = _criptografiaService.VerificarSenha(senha, hash);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void GerarHash_DeveGerarHashComFormatoCorreto()
    {
        // Arrange
        var senha = "TesteSenha@123";

        // Act
        var hash = _criptografiaService.GerarHash(senha);
        var parts = hash.Split('$');

        // Assert
        parts.Should().HaveCount(6);
        parts[0].Should().BeEmpty(); // Antes do primeiro $
        parts[1].Should().Be("argon2id");
        parts[2].Should().Be("v=19");
        parts[3].Should().Contain("m=").And.Contain("t=").And.Contain("p=");
        parts[4].Should().NotBeNullOrEmpty(); // Salt
        parts[5].Should().NotBeNullOrEmpty(); // Hash
    }

    [Fact]
    public void VerificarSenha_DeveLidarComSenhasVazias()
    {
        // Arrange
        var senhaVazia = "";
        var hash = _criptografiaService.GerarHash("outraSenha");

        // Act
        var resultado = _criptografiaService.VerificarSenha(senhaVazia, hash);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void VerificarSenha_DeveLidarComHashesVazios()
    {
        // Arrange
        var senha = "SenhaForte@123";
        var hashVazio = "";

        // Act
        var resultado = _criptografiaService.VerificarSenha(senha, hashVazio);

        // Assert
        resultado.Should().BeFalse();
    }
}
