using AgroSolutions.Propriedades.API.Extensions;
using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Propriedades.API.Controllers;

/// <summary>
/// Controller para gerenciamento de talhões
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Talhoes")]
public class TalhoesController : ControllerBase
{
    private readonly ITalhaoService _talhaoService;
    private readonly ILogger<TalhoesController> _logger;

    public TalhoesController(
        ITalhaoService talhaoService,
        ILogger<TalhoesController> logger)
    {
        _talhaoService = talhaoService;
        _logger = logger;
    }

    /// <summary>
    /// Criar novo talhão
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TalhaoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarTalhaoDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Criando talhão {Nome} na propriedade {PropriedadeId}", dto.Nome, dto.PropriedadeId);

        var resultado = await _talhaoService.CriarAsync(dto, usuarioId, ehAdmin, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, 
            ApiResponse<TalhaoDto>.Ok(resultado, "Talhão criado com sucesso."));
    }

    /// <summary>
    /// Obter talhão por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TalhaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _talhaoService.ObterPorIdAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<TalhaoDto>.Ok(resultado, "Talhão obtido com sucesso."));
    }

    /// <summary>
    /// Obter talhões de uma propriedade
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}")]
    [ProducesResponseType(typeof(ApiResponse<List<TalhaoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorPropriedade(Guid propriedadeId, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _talhaoService.ObterPorPropriedadeAsync(propriedadeId, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<List<TalhaoDto>>.Ok(resultado, $"{resultado.Count} talhão/talhões encontrado(s)."));
    }

    /// <summary>
    /// Obter talhões disponíveis de uma propriedade
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}/disponiveis")]
    [ProducesResponseType(typeof(ApiResponse<List<TalhaoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterDisponiveis(Guid propriedadeId, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _talhaoService.ObterDisponiveisAsync(propriedadeId, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<List<TalhaoDto>>.Ok(resultado, $"{resultado.Count} talhão/talhões disponível/disponíveis."));
    }

    /// <summary>
    /// Atualizar talhão
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TalhaoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarTalhaoDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Atualizando talhão {Id}", id);

        var resultado = await _talhaoService.AtualizarAsync(id, dto, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<TalhaoDto>.Ok(resultado, "Talhão atualizado com sucesso."));
    }

    /// <summary>
    /// Marcar talhão como em uso
    /// </summary>
    [HttpPatch("{id}/marcar-em-uso")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarcarComoEmUso(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        await _talhaoService.MarcarComoEmUsoAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Talhão marcado como em uso."));
    }

    /// <summary>
    /// Marcar talhão como disponível
    /// </summary>
    [HttpPatch("{id}/marcar-disponivel")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarcarComoDisponivel(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        await _talhaoService.MarcarComoDisponivelAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Talhão marcado como disponível."));
    }

    /// <summary>
    /// Marcar talhão como em descanso
    /// </summary>
    [HttpPatch("{id}/marcar-em-descanso")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarcarComoEmDescanso(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        await _talhaoService.MarcarComoEmDescansoAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Talhão marcado como em descanso."));
    }

    /// <summary>
    /// Remover talhão
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Removendo talhão {Id}", id);

        await _talhaoService.RemoverAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Talhão removido com sucesso."));
    }
}
