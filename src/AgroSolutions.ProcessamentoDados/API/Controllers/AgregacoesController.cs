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
public class AgregacoesController : ControllerBase
{
    private readonly IAgregacaoService _service;

    public AgregacoesController(IAgregacaoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Consulta agregações de dados
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<AgregacaoDadosDto>>), 200)]
    public async Task<IActionResult> Consultar([FromQuery] ConsultarAgregacoesDto filtros)
    {
        var agregacoes = await _service.ConsultarAgregacoesAsync(filtros);
        var lista = agregacoes.ToList();
        
        return Ok(ApiResponse<IEnumerable<AgregacaoDadosDto>>.Ok(lista
        , $"{lista.Count} agregação(ões) encontrada(s)"));
    }

    /// <summary>
    /// Obtém agregação por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<AgregacaoDadosDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var agregacao = await _service.ObterPorIdAsync(id);
        
        if (agregacao == null)
            return NotFound(ApiResponse<AgregacaoDadosDto>.Erro("Agregaçõo não encontrada"));

        return Ok(ApiResponse<AgregacaoDadosDto>.Ok(agregacao, "Agregação encontrada"));
    }

    /// <summary>
    /// Gera agregação horária manualmente
    /// </summary>
    [HttpPost("gerar-horaria")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GerarHoraria([FromBody] GerarAgregacaoDto dto)
    {
        await _service.GerarAgregacaoHorariaAsync(dto.SensorId, dto.PeriodoInicio);
        return Ok(ApiResponse<object>.Ok("Agregação horária gerada com sucesso"));
    }

    /// <summary>
    /// Gera agregação diária manualmente
    /// </summary>
    [HttpPost("gerar-diaria")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GerarDiaria([FromBody] GerarAgregacaoDto dto)
    {
        await _service.GerarAgregacaoDiariaAsync(dto.SensorId, dto.PeriodoInicio);
        return Ok(ApiResponse<object>.Ok("Agregação diária gerada com sucesso"));
    }

    /// <summary>
    /// Gera agregação semanal manualmente
    /// </summary>
    [HttpPost("gerar-semanal")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GerarSemanal([FromBody] GerarAgregacaoDto dto)
    {
        await _service.GerarAgregacaoSemanalAsync(dto.SensorId, dto.PeriodoInicio);
        return Ok(ApiResponse<object>.Ok("Agregação semanal gerada com sucesso"));
    }

    /// <summary>
    /// Gera agregação mensal manualmente
    /// </summary>
    [HttpPost("gerar-mensal")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GerarMensal([FromBody] GerarAgregacaoDto dto)
    {
        await _service.GerarAgregacaoMensalAsync(dto.SensorId, dto.PeriodoInicio);
        return Ok(ApiResponse<object>.Ok("Agregação mensal gerada com sucesso"));
    }
}
