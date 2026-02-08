using AgroSolutions.Identidade.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller para gerenciamento de chaves RSA (uso administrativo)
/// </summary>
[ApiController]
[Route("api/admin/[controller]")]
[Produces("application/json")]
[Tags("Administracao")]
public class ChavesController : ControllerBase
{
    private readonly RsaKeyManager _rsaKeyManager;
    private readonly ILogger<ChavesController> _logger;
    private readonly IConfiguration _configuration;

    public ChavesController(
        RsaKeyManager rsaKeyManager,
        ILogger<ChavesController> logger,
        IConfiguration configuration)
    {
        _rsaKeyManager = rsaKeyManager;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Regenera o par de chaves RSA
    /// Usar apenas em ambiente de desenvolvimento (invalida todos os tokens JWT existentes)
    /// </summary>
    [HttpPost("regenerar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult RegenerarChaves([FromHeader(Name = "X-Admin-Key")] string? adminKey)
    {
        // Proteção: apenas em desenvolvimento ou com chave admin
        var ambiente = _configuration["ASPNETCORE_ENVIRONMENT"];
        var adminKeyConfig = _configuration["AdminKey"];

        if (ambiente != "Development" && adminKey != adminKeyConfig)
        {
            _logger.LogWarning("Tentativa não autorizada de regenerar chaves RSA");
            return Forbid();
        }

        try
        {
            _rsaKeyManager.GenerateAndSaveKeys();
            _logger.LogWarning("Chaves RSA regeneradas! Todos os tokens JWT anteriores foram invalidados.");

            return Ok(new
            {
                mensagem = "Chaves RSA regeneradas com sucesso",
                aviso = "TODOS os tokens JWT existentes foram invalidados",
                jwksEndpoint = $"{Request.Scheme}://{Request.Host}/.well-known/jwks.json"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao regenerar chaves RSA");
            return StatusCode(500, new { erro = "Erro ao regenerar chaves" });
        }
    }

    /// <summary>
    /// Verifica o status das chaves RSA
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult VerificarStatus()
    {
        try
        {
            var rsa = _rsaKeyManager.GetRsa();
            var keySize = rsa.KeySize;

            return Ok(new
            {
                status = "OK",
                algoritmo = "RS256",
                tamanhoChave = keySize,
                diretorio = _configuration["RsaKeys:Directory"] ?? "keys",
                jwksEndpoint = $"{Request.Scheme}://{Request.Host}/.well-known/jwks.json"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status das chaves");
            return StatusCode(500, new { erro = "Erro ao verificar chaves", detalhes = ex.Message });
        }
    }
}
