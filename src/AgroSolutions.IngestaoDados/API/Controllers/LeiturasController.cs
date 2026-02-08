using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace AgroSolutions.IngestaoDados.API.Controllers;

/// <summary>
/// Controller para ingestão e consulta de leituras de sensores IoT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Leituras")]
public class LeiturasController : ControllerBase
{
    // MÉTRICAS PROMETHEUS
    private static readonly Counter LeiturasRecebidas = Metrics.CreateCounter(
        "agrosolutions_leituras_recebidas_total",
        "Total de leituras de sensores recebidas",
        new CounterConfiguration
        {
            LabelNames = new[] { "device_id", "unidade" }
        });

    private static readonly Counter LeituraLoteRecebido = Metrics.CreateCounter(
        "agrosolutions_leituras_lote_recebido_total",
        "Total de lotes de leituras recebidos",
        new CounterConfiguration
        {
            LabelNames = new[] { "quantidade" }
        });

    private static readonly Histogram TempoProcessamentoLeitura = Metrics.CreateHistogram(
        "agrosolutions_leitura_processamento_segundos",
        "Tempo de processamento de uma leitura",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    private readonly ILeituraService _leituraService;
    private readonly ILogger<LeiturasController> _logger;

    public LeiturasController(
        ILeituraService leituraService,
        ILogger<LeiturasController> logger)
    {
        _leituraService = leituraService;
        _logger = logger;
    }

    /// <summary>
    /// Ingerir leitura de sensor IoT (endpoint público para dispositivos)
    /// </summary>
    [HttpPost]
    [AllowAnonymous] // Sensores IoT não têm autenticação JWT
    [ProducesResponseType(typeof(ApiResponse<LeituraSensorDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarLeitura([FromBody] RegistrarLeituraDto dto, CancellationToken cancellationToken)
    {
        using (TempoProcessamentoLeitura.NewTimer())
        {
            _logger.LogInformation("Recebendo leitura: DeviceId={DeviceId}, Valor={Valor}{Unidade}",
                dto.DeviceId, dto.Valor, dto.Unidade);

            var resultado = await _leituraService.RegistrarLeituraAsync(dto, cancellationToken);

            // Incrementar métrica
            LeiturasRecebidas.WithLabels(dto.DeviceId, dto.Unidade).Inc();

            return Created(string.Empty, 
                ApiResponse<LeituraSensorDto>.Ok(resultado, "Leitura registrada com sucesso"));
        }
    }

    /// <summary>
    /// Ingerir lote de leituras de sensores (endpoint público para dispositivos)
    /// </summary>
    [HttpPost("lote")]
    [AllowAnonymous] // Sensores IoT não têm autenticação JWT
    [ProducesResponseType(typeof(ApiResponse<List<LeituraSensorDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegistrarLeituraLote([FromBody] RegistrarLeituraLoteDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recebendo lote de {Quantidade} leituras", dto.Leituras.Count);

        var resultado = await _leituraService.RegistrarLeituraLoteAsync(dto, cancellationToken);

        // Incrementar métrica de lote
        LeituraLoteRecebido.WithLabels(dto.Leituras.Count.ToString()).Inc();

        return Created(string.Empty, ApiResponse<List<LeituraSensorDto>>.Ok(resultado, $"{resultado.Count} leitura(s) registrada(s) com sucesso"));
    }

    /// <summary>
    /// Obter leitura por ID (autenticado)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LeituraSensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _leituraService.ObterPorIdAsync(id, cancellationToken);
        return Ok(ApiResponse<LeituraSensorDto>.Ok(resultado, "Leitura obtida com sucesso"));
    }

    /// <summary>
    /// Obter últimas leituras de um sensor (autenticado)
    /// </summary>
    [HttpGet("sensor/{sensorId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<LeituraSensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorSensor(Guid sensorId, [FromQuery] int limite = 100, CancellationToken cancellationToken = default)
    {
        var resultado = await _leituraService.ObterPorSensorAsync(sensorId, limite, cancellationToken);
        return Ok(ApiResponse<List<LeituraSensorDto>>.Ok(resultado, $"{resultado.Count} leitura(s) encontrada(s)"));
    }

    /// <summary>
    /// Obter última leitura de um sensor (autenticado)
    /// </summary>
    [HttpGet("sensor/{sensorId}/ultima")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LeituraSensorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterUltimaLeitura(Guid sensorId, CancellationToken cancellationToken)
    {
        var resultado = await _leituraService.ObterUltimaLeituraAsync(sensorId, cancellationToken);
        
        if (resultado == null)
            return NotFound(ApiResponse<object>.Erro("Nenhuma leitura encontrada para este sensor"));

        return Ok(ApiResponse<LeituraSensorDto>.Ok(resultado, "Última leitura obtida com sucesso"));
    }

    /// <summary>
    /// Obter leituras de uma propriedade em um período (autenticado)
    /// </summary>
    [HttpGet("propriedade/{propriedadeId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<LeituraSensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorPropriedade(
        Guid propriedadeId,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _leituraService.ObterPorPropriedadeAsync(propriedadeId, dataInicio, dataFim, cancellationToken);
        return Ok(ApiResponse<List<LeituraSensorDto>>.Ok(resultado, $"{resultado.Count} leitura(s) encontrada(s)"));
    }

    /// <summary>
    /// Obter leituras de um sensor em um período (autenticado)
    /// </summary>
    [HttpGet("sensor/{sensorId}/periodo")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<List<LeituraSensorDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterPorPeriodo(
        Guid sensorId,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _leituraService.ObterPorPeriodoAsync(sensorId, dataInicio, dataFim, cancellationToken);
        return Ok(ApiResponse<List<LeituraSensorDto>>.Ok(resultado, $"{resultado.Count} leitura(s) encontrada(s)"));
    }

    /// <summary>
    /// Obter estatísticas de leituras de um sensor (autenticado)
    /// </summary>
    [HttpGet("sensor/{sensorId}/estatisticas")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EstatisticasLeituraDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterEstatisticas(
        Guid sensorId,
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        var resultado = await _leituraService.ObterEstatisticasAsync(sensorId, dataInicio, dataFim, cancellationToken);
        return Ok(ApiResponse<EstatisticasLeituraDto>.Ok(resultado, "Estatísticas calculadas com sucesso"));
    }

    /// <summary>
    /// Marcar leitura como suspeita (autenticado)
    /// </summary>
    [HttpPatch("{id}/suspeita")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoSuspeita(Guid id, [FromBody] string motivo, CancellationToken cancellationToken)
    {
        await _leituraService.MarcarComoSuspeitaAsync(id, motivo, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Leitura marcada como suspeita"));
    }

    /// <summary>
    /// Marcar leitura como inválida (autenticado)
    /// </summary>
    [HttpPatch("{id}/invalida")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarcarComoInvalida(Guid id, [FromBody] string motivo, CancellationToken cancellationToken)
    {
        await _leituraService.MarcarComoInvalidaAsync(id, motivo, cancellationToken);
        return Ok(ApiResponse<object>.Ok("Leitura marcada como inválida"));
    }
}

