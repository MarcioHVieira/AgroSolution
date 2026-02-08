using AgroSolutions.Notificacoes.Application.DTOs;
using AgroSolutions.Notificacoes.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Notificacoes.API.Controllers;

/// <summary>
/// Controller de Notificações
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Notificacoes")]
public class NotificacoesController : ControllerBase
{
    private readonly INotificacaoService _service;
    private readonly ILogger<NotificacoesController> _logger;

    public NotificacoesController(INotificacaoService service, ILogger<NotificacoesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Obter todas as notificações
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Tecnico")]
    [ProducesResponseType(typeof(IEnumerable<NotificacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificacaoDto>>> ObterTodas() => 
        Ok(await _service.ObterTodasAsync());

    /// <summary>
    /// Obter notificação por ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(NotificacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificacaoDto>> ObterPorId(Guid id)
    {
        var result = await _service.ObterPorIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Obter notificações por destinatário
    /// </summary>
    [HttpGet("destinatario/{destinatarioId}")]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<NotificacaoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<NotificacaoDto>>> ObterPorDestinatario(Guid destinatarioId) =>
        Ok(await _service.ObterPorDestinatarioAsync(destinatarioId));

    /// <summary>
    /// Obter estatísticas de notificações
    /// </summary>
    [HttpGet("estatisticas")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(EstatisticasNotificacoesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstatisticasNotificacoesDto>> ObterEstatisticas() =>
        Ok(await _service.ObterEstatisticasAsync());

    /// <summary>
    /// Criar notificação manual
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Tecnico")]
    [ProducesResponseType(typeof(NotificacaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificacaoDto>> Criar([FromBody] CriarNotificacaoDto dto)
    {
        var result = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }
}

