using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Configuration.Settings;
using AgroSolutions.Identidade.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AgroSolutions.Identidade.Infrastructure.Services;

/// <summary>
/// Serviço para geração de tokens JWT e Refresh Tokens
/// Utiliza assinatura RSA (RS256) com chave pública/privada
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly RsaKeyManager _rsaKeyManager;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOptions<JwtSettings> jwtSettings, 
        RsaKeyManager rsaKeyManager,
        ILogger<TokenService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _rsaKeyManager = rsaKeyManager;
        _logger = logger;
        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Issuer))
            throw new InvalidOperationException("JWT Issuer não configurado.");

        if (string.IsNullOrWhiteSpace(_jwtSettings.Audience))
            throw new InvalidOperationException("JWT Audience não configurado.");
    }

    public string GerarToken(Guid usuarioId, string email, string perfil)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Assinatura com RSA (RS256)
        var rsa = _rsaKeyManager.GetRsa();
        var rsaParameters = rsa.ExportParameters(includePrivateParameters: false);
        
        // Gera o mesmo kid que está no JWKS
        var keyId = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(rsaParameters.Modulus!)
        ).Substring(0, 16);
        
        var securityKey = new RsaSecurityKey(rsa) 
        { 
            KeyId = keyId 
        };
        
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);
        
        _logger.LogDebug("Token JWT assinado com RSA (RS256) usando KeyId: {KeyId}", keyId);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiracaoMinutos),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GerarRefreshToken()
    {
        // Gera um token aleatório criptograficamente seguro de 64 bytes (512 bits)
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}




