using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Configuration.Settings;
using AgroSolutions.Identidade.Infrastructure.Security;
using AgroSolutions.Identidade.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace AgroSolutions.Identidade.Test.Infrastructure.Services;

public class TokenServiceTests
{
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
    private readonly RsaKeyManager _rsaKeyManager;
    private readonly Mock<ILogger<TokenService>> _loggerMock;
    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {
        _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        _loggerMock = new Mock<ILogger<TokenService>>();
        
        var jwtSettings = new JwtSettings
        {
            Issuer = "AgroSolutions.Test",
            Audience = "AgroSolutions.Test.API",
            ExpiracaoMinutos = 60
        };
        _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

        // Criar RsaKeyManager real com configuration mockada
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["RsaKeys:Directory"]).Returns(Path.Combine(Path.GetTempPath(), "test-keys-" + Guid.NewGuid()));
        _rsaKeyManager = new RsaKeyManager(configurationMock.Object);

        _tokenService = new TokenService(_jwtSettingsMock.Object, _rsaKeyManager, _loggerMock.Object);
    }

    [Fact]
    public void Construtor_DeveLancarExcecaoQuandoIssuerNaoConfigurado()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Issuer = "",
            Audience = "AgroSolutions.Test.API",
            ExpiracaoMinutos = 60
        };
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

        // Act
        var act = () => new TokenService(jwtSettingsMock.Object, _rsaKeyManager, _loggerMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Issuer*");
    }

    [Fact]
    public void Construtor_DeveLancarExcecaoQuandoAudienceNaoConfigurado()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Issuer = "AgroSolutions.Test",
            Audience = "",
            ExpiracaoMinutos = 60
        };
        var jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

        // Act
        var act = () => new TokenService(jwtSettingsMock.Object, _rsaKeyManager, _loggerMock.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Audience*");
    }

    [Fact]
    public void GerarToken_DeveRetornarTokenValido()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "marcio@agrosolutions.com.br";
        var perfil = "Usuario";

        // Act
        var token = _tokenService.GerarToken(usuarioId, email, perfil);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GerarToken_DeveIncluirClaimsCorretos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "marcio@agrosolutions.com.br";
        var perfil = "Usuario";

        // Act
        var token = _tokenService.GerarToken(usuarioId, email, perfil);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == usuarioId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == perfil);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    public void GerarToken_DeveIncluirIssuerEAudienceCorretos()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "marcio@agrosolutions.com.br";
        var perfil = "Usuario";

        // Act
        var token = _tokenService.GerarToken(usuarioId, email, perfil);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be("AgroSolutions.Test");
        jwtToken.Audiences.Should().Contain("AgroSolutions.Test.API");
    }

    [Fact]
    public void GerarToken_DeveDefinirDataExpiracao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "marcio@agrosolutions.com.br";
        var perfil = "Usuario";

        // Act
        var token = _tokenService.GerarToken(usuarioId, email, perfil);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void GerarRefreshToken_DeveRetornarTokenNaoVazio()
    {
        // Act
        var refreshToken = _tokenService.GerarRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GerarRefreshToken_DeveGerarTokensDiferentes()
    {
        // Act
        var refreshToken1 = _tokenService.GerarRefreshToken();
        var refreshToken2 = _tokenService.GerarRefreshToken();

        // Assert
        refreshToken1.Should().NotBe(refreshToken2);
    }

    [Fact]
    public void GerarRefreshToken_DeveGerarTokenBase64Valido()
    {
        // Act
        var refreshToken = _tokenService.GerarRefreshToken();

        // Assert
        var act = () => Convert.FromBase64String(refreshToken);
        act.Should().NotThrow();
    }

    [Fact]
    public void GerarToken_DeveGerarTokensDiferentesParaMesmoUsuario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var email = "marcio@agrosolutions.com.br";
        var perfil = "Usuario";

        // Act
        var token1 = _tokenService.GerarToken(usuarioId, email, perfil);
        var token2 = _tokenService.GerarToken(usuarioId, email, perfil);

        // Assert
        token1.Should().NotBe(token2); // Diferentes devido ao JTI
    }
}
