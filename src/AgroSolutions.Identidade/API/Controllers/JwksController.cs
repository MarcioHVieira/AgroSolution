using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Identidade.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller para expor chaves públicas para validação de tokens JWT
/// </summary>
[ApiController]
[Route(".well-known")]
[Produces("application/json")]
[Tags("Chaves Publicas")]
public class JwksController : ControllerBase
{
    private readonly RsaKeyManager _rsaKeyManager;
    private readonly IConfiguration _configuration;

    public JwksController(RsaKeyManager rsaKeyManager, IConfiguration configuration)
    {
        _rsaKeyManager = rsaKeyManager;
        _configuration = configuration;
    }

    /// <summary>
    /// OpenID Connect Discovery Document
    /// Endpoint padrão que outros serviços usam para descobrir configurações
    /// </summary>
    [HttpGet("openid-configuration")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetOpenIdConfiguration()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var config = new
        {
            issuer = _configuration["Jwt:Issuer"] ?? "AgroSolutions.Identidade",
            jwks_uri = $"{baseUrl}/.well-known/jwks.json",
            authorization_endpoint = $"{baseUrl}/api/autenticacao/login",
            token_endpoint = $"{baseUrl}/api/autenticacao/login",
            token_endpoint_auth_methods_supported = new[] { "client_secret_post", "client_secret_basic" },
            response_types_supported = new[] { "code", "token", "id_token" },
            subject_types_supported = new[] { "public" },
            id_token_signing_alg_values_supported = new[] { "RS256" },
            scopes_supported = new[] { "openid", "profile", "email" }
        };

        // Retorna JSON direto sem ApiResponse wrapper (padrão OpenID Connect)
        return new JsonResult(config);
    }

    /// <summary>
    /// Endpoint JWKS (JSON Web Key Set) - Padrão OpenID Connect
    /// Outros microserviços podem usar este endpoint para validar tokens JWT
    /// </summary>
    [HttpGet("jwks.json")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetJwks()
    {
        var jwk = _rsaKeyManager.GetPublicKeyJwk();
        
        var jwks = new
        {
            keys = new[] { jwk }
        };

        // Retorna JSON direto sem ApiResponse wrapper (padrão OpenID Connect)
        return new JsonResult(jwks);
    }

    /// <summary>
    /// Obtém a chave pública em formato PEM
    /// Útil para integração com sistemas que não suportam JWKS
    /// </summary>
    [HttpGet("public-key")]
    [AllowAnonymous]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetPublicKeyPem()
    {
        var publicKeyPem = _rsaKeyManager.GetPublicKeyPem();
        return Content(publicKeyPem, "text/plain");
    }

    /// <summary>
    /// Obtém a chave pública em formato XML
    /// </summary>
    [HttpGet("public-key-xml")]
    [AllowAnonymous]
    [Produces("application/xml")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetPublicKeyXml()
    {
        var publicKeyXml = _rsaKeyManager.GetPublicKeyXml();
        return Content(publicKeyXml, "application/xml");
    }

    /// <summary>
    /// Informaçães sobre a configuração de chaves
    /// </summary>
    [HttpGet("key-info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult GetKeyInfo()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        var info = new
        {
            algoritmo = "RS256",
            tipo = "RSA",
            tamanho = 2048,
            uso = "Assinatura de tokens JWT",
            openidConfigEndpoint = $"{baseUrl}/.well-known/openid-configuration",
            jwksEndpoint = $"{baseUrl}/.well-known/jwks.json",
            publicKeyPemEndpoint = $"{baseUrl}/.well-known/public-key"
        };

        return Ok(ApiResponse<object>.Ok(info, "Informações sobre as chaves públicas"));
    }
}
