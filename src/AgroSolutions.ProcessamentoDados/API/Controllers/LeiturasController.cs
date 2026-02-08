using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.SharedKernel.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.ProcessamentoDados.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LeiturasController : ControllerBase
{
    private readonly IProcessamentoService _service;

    public LeiturasController(IProcessamentoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Consulta leituras processadas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<LeituraProcessadaDto>>), 200)]
    public async Task<IActionResult> Consultar([FromQuery] ConsultarLeiturasDto filtros)
    {
        var leituras = await _service.ConsultarLeiturasAsync(filtros);
        var lista = leituras.ToList();
        
        return Ok(ApiResponse<IEnumerable<LeituraProcessadaDto>>.Ok(lista
        , $"{lista.Count} leitura(s) encontrada(s)"));
    }

    /// <summary>
    /// Obtém leitura por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<LeituraProcessadaDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var leitura = await _service.ObterPorIdAsync(id);
        
        if (leitura == null)
            return NotFound(ApiResponse<LeituraProcessadaDto>.Erro("Leitura não encontrada"));

        return Ok(ApiResponse<LeituraProcessadaDto>.Ok(leitura, "Leitura encontrada"));
    }

    /// <summary>
    /// Obtém estatísticas de processamento
    /// </summary>
    [HttpGet("estatisticas")]
    [ProducesResponseType(typeof(ApiResponse<EstatisticasProcessamentoDto>), 200)]
    public async Task<IActionResult> ObterEstatisticas(
        [FromQuery] DateTime? dataInicio = null,
        [FromQuery] DateTime? dataFim = null)
    {
        var inicio = dataInicio ?? DateTime.UtcNow.AddDays(-7);
        var fim = dataFim ?? DateTime.UtcNow;

        var estatisticas = await _service.ObterEstatisticasAsync(inicio, fim);

        return Ok(ApiResponse<EstatisticasProcessamentoDto>.Ok(estatisticas
        , "Estatísticas obtidas com sucesso"));
    }

    /// <summary>
    /// Reprocessa leituras com falha
    /// </summary>
    [HttpPost("reprocessar-falhas")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> ReprocessarFalhas([FromQuery] int limite = 100)
    {
        await _service.ReprocessarFalhasAsync(limite);

        return Ok(ApiResponse<object>.Ok($"Reprocessamento iniciado (limite: {limite})"));
    }
}
