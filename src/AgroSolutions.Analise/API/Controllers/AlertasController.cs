using AgroSolutions.Analise.Application.DTOs;
using AgroSolutions.Analise.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Analise.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Alertas")]
public class AlertasController : ControllerBase
{
    private readonly IAlertaService _alertaService;
    private readonly ILogger<AlertasController> _logger;

    public AlertasController(
        IAlertaService alertaService,
        ILogger<AlertasController> logger)
    {
        _alertaService = alertaService;
        _logger = logger;
    }

    /// <summary>
    /// Obter alerta por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlertaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlertaDto>> ObterPorId(Guid id)
    {
        var alerta = await _alertaService.ObterPorIdAsync(id);

        if (alerta == null)
            return NotFound($"Alerta {id} não encontrado");

        return Ok(alerta);
    }

    /// <summary>
    /// Obter todos os alertas de um talhão
    /// </summary>
    [HttpGet("talhao/{talhaoId}")]
    [ProducesResponseType(typeof(IEnumerable<AlertaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertaDto>>> ObterPorTalhao(Guid talhaoId)
    {
        var alertas = await _alertaService.ObterTodosPorTalhaoAsync(talhaoId);
        return Ok(alertas);
    }

    /// <summary>
    /// Obter todos os alertas ativos
    /// </summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<AlertaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AlertaDto>>> ObterAtivos()
    {
        var alertas = await _alertaService.ObterAtivosAsync();
        return Ok(alertas);
    }

    /// <summary>
    /// Criar novo alerta manualmente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Tecnico")]
    [ProducesResponseType(typeof(AlertaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlertaDto>> Criar([FromBody] CriarAlertaDto dto)
    {
        var alerta = await _alertaService.CriarAsync(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = alerta.Id }, alerta);
    }

    /// <summary>
    /// Atualizar status do alerta
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] AtualizarStatusAlertaDto dto)
    {
        await _alertaService.AtualizarStatusAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Marcar alerta como visualizado
    /// </summary>
    [HttpPut("{id}/visualizar")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarComoVisualizado(Guid id)
    {
        await _alertaService.MarcarComoVisualizadoAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Marcar alerta como resolvido
    /// </summary>
    [HttpPut("{id}/resolver")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarcarComoResolvido(Guid id)
    {
        await _alertaService.MarcarComoResolvidoAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Obter estatísticas de alertas
    /// </summary>
    [HttpGet("estatisticas")]
    [Authorize]
    [ProducesResponseType(typeof(EstatisticasAlertasDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EstatisticasAlertasDto>> ObterEstatisticas()
    {
        var estatisticas = await _alertaService.ObterEstatisticasAsync();
        return Ok(estatisticas);
    }
}
