using AgroSolutions.SharedKernel.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller para verificação de saúde e status do serviço
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Saude")]
public class SaudeController : ControllerBase

{
    /// <summary>
    /// Health check básico
    /// </summary>
    /// <returns>Status do serviço</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var data = new
        {
            servico = "AgroSolutions.Identidade",
            status = "Operacional",
            versao = "1.0.0",
            timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Serviço operacional."));
    }
}

