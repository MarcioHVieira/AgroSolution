using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.IngestaoDados.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.IngestaoDados.API.Controllers;

/// <summary>
/// Controller para gerenciamento de sensores IoT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Sensores")]
public class SensoresController : ControllerBase
{
    private readonly ISensorService _sensorService;
    private readonly ILogger<SensoresController> _logger;

    public SensoresController(
        ISensorService sensorService,
        ILogger<SensoresController> logger)
    {
        _sensorService = sensorService;
        _logger = logger;
    }

    /// <summary>
    /// Cadastrar novo sensor IoT
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CriarSensorDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cadastrando sensor: DeviceId={DeviceId}, Tipo={Tipo}", dto.DeviceId, dto.Tipo);

        var resultado = await _sensorService.CriarAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id },
            ApiResponse<SensorDto>.Ok(resultado, "Sensor cadastrado com sucesso"));
    }

    /// <summary>
    /// Obter sensor por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<SensorDto>.Ok(resultado, "Sensor obtido com sucesso"));
    }

    /// <summary>
    /// Obter sensor por DeviceId
    /// </summary>
    [HttpGet("device/{deviceId}")]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorDeviceId(string deviceId, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterPorDeviceIdAsync(deviceId, cancellationToken);
        return Ok(ApiResponse<SensorDto>.Ok(resultado, "Sensor obtido com sucesso"));
    }

    /// <summary>
    /// Listar sensores de uma propriedade
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}")]
    [ProducesResponseType(typeof(ApiResponse<List<SensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorPropriedade(Guid propriedadeId, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterPorPropriedadeAsync(propriedadeId, cancellationToken);
        return Ok(ApiResponse<List<SensorDto>>.Ok(resultado, $"{resultado.Count} sensor(es) encontrado(s)"));
    }

    /// <summary>
    /// Listar sensores de um talhão
    /// </summary>
    [HttpGet("talhao/{talhaoId}")]
    [ProducesResponseType(typeof(ApiResponse<List<SensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorTalhao(Guid talhaoId, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterPorTalhaoAsync(talhaoId, cancellationToken);
        return Ok(ApiResponse<List<SensorDto>>.Ok(resultado, $"{resultado.Count} sensor(es) encontrado(s)"));
    }

    /// <summary>
    /// Listar sensores por tipo
    /// </summary>
    [HttpGet("tipo/{tipo}")]
    [ProducesResponseType(typeof(ApiResponse<List<SensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorTipo(TipoSensor tipo, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterPorTipoAsync(tipo, cancellationToken);
        return Ok(ApiResponse<List<SensorDto>>.Ok(resultado, $"{resultado.Count} sensor(es) encontrado(s)"));
    }

    /// <summary>
    /// Listar sensores ativos de uma propriedade
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}/ativos")]
    [ProducesResponseType(typeof(ApiResponse<List<SensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAtivos(Guid propriedadeId, CancellationToken cancellationToken)
    {
        var resultado = await _sensorService.ObterAtivosPorPropriedadeAsync(propriedadeId, cancellationToken);
        return Ok(ApiResponse<List<SensorDto>>.Ok(resultado, $"{resultado.Count} sensor(es) ativo(s)"));
    }

    /// <summary>
    /// Atualizar sensor
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarSensorDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando sensor {Id}", id);

        var resultado = await _sensorService.AtualizarAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<SensorDto>.Ok(resultado, "Sensor atualizado com sucesso"));
    }

    /// <summary>
    /// Atualizar status do sensor
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] StatusSensor status, CancellationToken cancellationToken)
    {
        await _sensorService.AtualizarStatusAsync(id, status, cancellationToken);
        return Ok(ApiResponse<object>.Ok($"Status do sensor alterado para {status}"));
    }

    /// <summary>
    /// Registrar calibração do sensor
    /// </summary>
    [HttpPost("{id}/calibracao")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarCalibracao(Guid id, CancellationToken cancellationToken)
    {
        await _sensorService.RegistrarCalibracaoAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Calibração registrada com sucesso"));
    }

    /// <summary>
    /// Remover sensor
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removendo sensor {Id}", id);

        await _sensorService.RemoverAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Sensor removido com sucesso"));
    }
}

