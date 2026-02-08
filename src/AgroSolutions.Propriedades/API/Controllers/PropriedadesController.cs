using AgroSolutions.Propriedades.API.Extensions;
using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Propriedades.API.Controllers;

/// <summary>
/// Controller para gerenciamento de propriedades rurais
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Propriedades")]
public class PropriedadesController : ControllerBase
{
    private readonly IPropriedadeService _propriedadeService;
    private readonly ILogger<PropriedadesController> _logger;

    public PropriedadesController(
        IPropriedadeService propriedadeService,
        ILogger<PropriedadesController> logger)
    {
        _propriedadeService = propriedadeService;
        _logger = logger;
    }

    /// <summary>
    /// Criar nova propriedade
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PropriedadeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarPropriedadeDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        _logger.LogInformation("Criando propriedade {Nome} para usuário {UsuarioId}", dto.Nome, usuarioId);

        var resultado = await _propriedadeService.CriarAsync(usuarioId, dto, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id }, 
            ApiResponse<PropriedadeDto>.Ok(resultado, "Propriedade criada com sucesso."));
    }

    /// <summary>
    /// Obter propriedade por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PropriedadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _propriedadeService.ObterPorIdAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<PropriedadeDto>.Ok(resultado, "Propriedade obtida com sucesso."));
    }

    /// <summary>
    /// Obter propriedades do usuário autenticado
    /// </summary>
    [HttpGet("minhas")]
    [ProducesResponseType(typeof(ApiResponse<List<PropriedadeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterMinhas(CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        var resultado = await _propriedadeService.ObterPorProprietarioAsync(usuarioId, cancellationToken);
        return Ok(ApiResponse<List<PropriedadeDto>>.Ok(resultado, $"{resultado.Count} propriedade(s) encontrada(s)."));
    }

    /// <summary>
    /// Listar todas as propriedades (administrador vê todas, usuário comum vê apenas as suas)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<PropriedadeDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodas([FromQuery] int pagina = 1, [FromQuery] int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _propriedadeService.ObterTodasAsync(usuarioId, ehAdmin, pagina, tamanhoPagina, cancellationToken);
        return Ok(ApiResponse<List<PropriedadeDto>>.Ok(resultado, $"{resultado.Count} propriedade(s) encontrada(s)."));
    }

    /// <summary>
    /// Atualizar propriedade
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<PropriedadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPropriedadeDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        _logger.LogInformation("Atualizando propriedade {Id}", id);

        var resultado = await _propriedadeService.AtualizarAsync(id, dto, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<PropriedadeDto>.Ok(resultado, "Propriedade atualizada com sucesso."));
    }

    /// <summary>
    /// Atualizar endereço da propriedade
    /// </summary>
    [HttpPut("{id}/endereco")]
    [ProducesResponseType(typeof(ApiResponse<PropriedadeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AtualizarEndereco(Guid id, [FromBody] AtualizarEnderecoPropriedadeDto dto, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        var resultado = await _propriedadeService.AtualizarEnderecoAsync(id, dto, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<PropriedadeDto>.Ok(resultado, "Endereço atualizado com sucesso."));
    }

    /// <summary>
    /// Ativar propriedade
    /// </summary>
    [HttpPatch("{id}/ativar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        await _propriedadeService.AtivarAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Propriedade ativada com sucesso."));
    }

    /// <summary>
    /// Inativar propriedade
    /// </summary>
    [HttpPatch("{id}/inativar")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var ehAdmin = User.EhAdministrador();

        await _propriedadeService.InativarAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Propriedade inativada com sucesso."));
    }

    /// <summary>
    /// Remover propriedade
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

        _logger.LogInformation("Removendo propriedade {Id}", id);

        await _propriedadeService.RemoverAsync(id, usuarioId, ehAdmin, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Propriedade removida com sucesso."));
    }
}

