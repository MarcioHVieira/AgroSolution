using AgroSolutions.Propriedades.API.Extensions;
using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Propriedades.API.Controllers;

/// <summary>
/// Controller para gerenciamento de culturas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Culturas")]
public class CulturasController : ControllerBase
{
    private readonly ICulturaService _culturaService;
    private readonly ILogger<CulturasController> _logger;

    public CulturasController(
        ICulturaService culturaService,
        ILogger<CulturasController> logger)
    {
        _culturaService = culturaService;
        _logger = logger;
    }

    /// <summary>
    /// Criar nova cultura
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CulturaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarCulturaDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Criando cultura {Tipo} no talhão {TalhaoId}", dto.Tipo, dto.TalhaoId);

        var resultado = await _culturaService.CriarAsync(dto, usuarioId, ehAdmin, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, 
            ApiResponse<CulturaDto>.Ok(resultado, "Cultura criada com sucesso."));
    }

    /// <summary>
    /// Obter cultura por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CulturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _culturaService.ObterPorIdAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<CulturaDto>.Ok(resultado, "Cultura obtida com sucesso."));
    }

    /// <summary>
    /// Obter culturas de um talhão
    /// </summary>
    [HttpGet("talhao/{talhaoId}")]
    [ProducesResponseType(typeof(ApiResponse<List<CulturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorTalhao(Guid talhaoId, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _culturaService.ObterPorTalhaoAsync(talhaoId, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<List<CulturaDto>>.Ok(resultado, $"{resultado.Count} cultura(s) encontrada(s)."));
    }

    /// <summary>
    /// Obter culturas de uma propriedade
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}")]
    [ProducesResponseType(typeof(ApiResponse<List<CulturaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorPropriedade(Guid propriedadeId, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _culturaService.ObterPorPropriedadeAsync(propriedadeId, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<List<CulturaDto>>.Ok(resultado, $"{resultado.Count} cultura(s) encontrada(s)."));
    }

    /// <summary>
    /// Obter culturas ativas (administrador vê todas, usuário comum vê apenas as suas)
    /// </summary>
    [HttpGet("ativas")]
    [ProducesResponseType(typeof(ApiResponse<List<CulturaDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAtivas(CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _culturaService.ObterAtivasAsync(usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<List<CulturaDto>>.Ok(resultado, $"{resultado.Count} cultura(s) ativa(s)."));
    }

    /// <summary>
    /// Atualizar cultura
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CulturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarCulturaDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Atualizando cultura {Id}", id);

        var resultado = await _culturaService.AtualizarAsync(id, dto, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<CulturaDto>.Ok(resultado, "Cultura atualizada com sucesso."));
    }

    /// <summary>
    /// Registrar colheita
    /// </summary>
    [HttpPost("{id}/colheita")]
    [ProducesResponseType(typeof(ApiResponse<CulturaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegistrarColheita(Guid id, [FromBody] RegistrarColheitaDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Registrando colheita para cultura {Id}", id);

        var resultado = await _culturaService.RegistrarColheitaAsync(id, dto, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<CulturaDto>.Ok(resultado, "Colheita registrada com sucesso."));
    }

    /// <summary>
    /// Cancelar cultura
    /// </summary>
    [HttpPost("{id}/cancelar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Cancelar(Guid id, [FromBody] string motivo, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Cancelando cultura {Id}", id);

        await _culturaService.CancelarAsync(id, motivo, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Cultura cancelada com sucesso."));
    }

    /// <summary>
    /// Remover cultura
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Removendo cultura {Id}", id);

        await _culturaService.RemoverAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Cultura removida com sucesso."));
    }
}
