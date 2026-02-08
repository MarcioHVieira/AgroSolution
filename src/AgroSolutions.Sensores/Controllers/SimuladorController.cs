using AgroSolutions.Sensores.Models;
using AgroSolutions.Sensores.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Sensores.Controllers;

[ApiController]
[Route("api/simulador")]
[Produces("application/json")]
public class SimuladorController : ControllerBase
{
    private readonly ISimuladorService _simuladorService;
    private readonly ILogger<SimuladorController> _logger;

    public SimuladorController(
        ISimuladorService simuladorService,
        ILogger<SimuladorController> logger)
    {
        _simuladorService = simuladorService;
        _logger = logger;
    }

    /// <summary>
    /// Simula cenário de SECA (umidade do solo abaixo de 30%)
    /// </summary>
    /// <param name="request">Dados da simulação (talhaoId, severidade e se deve enviar para API)</param>
    /// <returns>Resultado detalhado da simulação incluindo todas as leituras geradas</returns>
    /// <response code="200">Simulação executada com sucesso. Retorna todas as leituras geradas e estatísticas.</response>
    /// <response code="400">Dados de entrada inválidos (severidade desconhecida ou talhaoId inválido)</response>
    /// <response code="500">Erro ao processar simulação (verificar logs para detalhes)</response>
    [HttpPost("seca")]
    [EndpointSummary("Simula cenário de SECA (umidade do solo abaixo de 30%)")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularSeca([FromBody] SimulacaoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de SECA recebida - TalhaoId: {TalhaoId}, Severidade: {Severidade}", 
                request.TalhaoId, request.Severidade);

            if (!Enum.TryParse<Severidade>(request.Severidade, true, out var severidade))
            {
                return BadRequest($"Severidade inválida. Valores permitidos: Normal, Media, Alta, Critica");
            }

            var resultado = await _simuladorService.SimularSecaAsync(request.TalhaoId, severidade, request.EnviarParaApi);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário de seca");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Simula cenário de GEADA (temperatura abaixo de 2°C)
    /// </summary>
    /// <param name="request">Dados da simulação (talhaoId, severidade e se deve enviar para API)</param>
    /// <returns>Resultado detalhado da simulação com todas as leituras de temperatura geradas</returns>
    /// <response code="200">Simulação de geada executada com sucesso</response>
    /// <response code="400">Severidade inválida ou talhaoId não encontrado</response>
    /// <response code="500">Erro ao processar simulação (verificar conexão com API de ingestão)</response>
    [HttpPost("geada")]
    [EndpointSummary("Simula cenário de GEADA (temperatura abaixo de 2°C)")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularGeada([FromBody] SimulacaoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de GEADA recebida - TalhaoId: {TalhaoId}, Severidade: {Severidade}", 
                request.TalhaoId, request.Severidade);

            if (!Enum.TryParse<Severidade>(request.Severidade, true, out var severidade))
            {
                return BadRequest($"Severidade inválida. Valores permitidos: Normal, Media, Alta, Critica");
            }

            var resultado = await _simuladorService.SimularGeadaAsync(request.TalhaoId, severidade, request.EnviarParaApi);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário de geada");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Simula cenário de CALOR EXCESSIVO (temperatura alta)
    /// </summary>
    /// <param name="request">Dados da simulação (talhaoId, severidade e se deve enviar para API)</param>
    /// <returns>Resultado detalhado da simulação com todas as leituras de temperatura geradas</returns>
    /// <response code="200">Simulação de calor excessivo executada com sucesso</response>
    /// <response code="400">Severidade inválida. Use: Normal, Media, Alta ou Critica</response>
    /// <response code="500">Erro ao processar simulação ou enviar dados</response>
    [HttpPost("calor-excessivo")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularCalorExcessivo([FromBody] SimulacaoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de CALOR EXCESSIVO recebida - TalhaoId: {TalhaoId}, Severidade: {Severidade}", 
                request.TalhaoId, request.Severidade);

            if (!Enum.TryParse<Severidade>(request.Severidade, true, out var severidade))
            {
                return BadRequest($"Severidade inválida. Valores permitidos: Normal, Media, Alta, Critica");
            }

            var resultado = await _simuladorService.SimularCalorExcessivoAsync(request.TalhaoId, severidade, request.EnviarParaApi);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário de calor excessivo");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Simula cenário de EXCESSO DE UMIDADE (solo encharcado)
    /// </summary>
    /// <param name="request">Dados da simulação (talhaoId, severidade e se deve enviar para API)</param>
    /// <returns>Resultado detalhado da simulação com todas as leituras de umidade geradas ao longo de 48h</returns>
    /// <response code="200">Simulação de excesso de umidade executada com sucesso</response>
    /// <response code="400">Dados inválidos ou talhão não encontrado</response>
    /// <response code="500">Erro ao processar ou comunicar com microsserviços</response>
    [HttpPost("excesso-umidade")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularExcessoUmidade([FromBody] SimulacaoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de EXCESSO DE UMIDADE recebida - TalhaoId: {TalhaoId}, Severidade: {Severidade}", 
                request.TalhaoId, request.Severidade);

            if (!Enum.TryParse<Severidade>(request.Severidade, true, out var severidade))
            {
                return BadRequest($"Severidade inválida. Valores permitidos: Normal, Media, Alta, Critica");
            }

            var resultado = await _simuladorService.SimularExcessoUmidadeAsync(request.TalhaoId, severidade, request.EnviarParaApi);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário de excesso de umidade");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Simula cenário de RISCO DE PRAGA (condições ideais)
    /// </summary>
    /// <param name="request">Dados da simulação (talhaoId, severidade e se deve enviar para API)</param>
    /// <returns>Resultado detalhado com 48 leituras (24 temperatura + 24 umidade) ao longo de 48h</returns>
    /// <response code="200">Simulação de risco de praga executada com sucesso. Retorna leituras de temperatura e umidade.</response>
    /// <response code="400">Severidade inválida ou talhaoId não existe no sistema</response>
    /// <response code="500">Erro ao processar simulação ou enviar para API de ingestão</response>
    [HttpPost("risco-praga")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularRiscoPraga([FromBody] SimulacaoRequestDto request)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de RISCO DE PRAGA recebida - TalhaoId: {TalhaoId}, Severidade: {Severidade}", 
                request.TalhaoId, request.Severidade);

            if (!Enum.TryParse<Severidade>(request.Severidade, true, out var severidade))
            {
                return BadRequest($"Severidade inválida. Valores permitidos: Normal, Media, Alta, Critica");
            }

            var resultado = await _simuladorService.SimularRiscoPragaAsync(request.TalhaoId, severidade, request.EnviarParaApi);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário de risco de praga");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Simula TODOS os cenários em sequência (teste completo do sistema)
    /// </summary>
    /// <param name="talhaoId">ID (GUID) do talhão onde a simulação será executada. Deve ser um talhão válido cadastrado no sistema.</param>
    /// <returns>Resultado consolidado com todas as 120 leituras geradas e estatísticas globais</returns>
    /// <response code="200">Cenário completo executado com sucesso! Todos os 5 cenários foram simulados.</response>
    /// <response code="400">TalhaoId inválido ou não encontrado no sistema</response>
    /// <response code="500">Erro durante execução de algum cenário. Verificar logs para detalhes.</response>
    /// <response code="504">Gateway Timeout - Simulação está demorando mais que o esperado, mas pode ainda estar em execução</response>
    [HttpPost("cenario-completo/{talhaoId}")]
    [ProducesResponseType(typeof(ResultadoSimulacaoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<ResultadoSimulacaoDto>> SimularCenarioCompleto([FromRoute] Guid talhaoId)
    {
        try
        {
            _logger.LogInformation("Requisição de simulação de CENÁRIO COMPLETO recebida - TalhaoId: {TalhaoId}", talhaoId);

            var resultado = await _simuladorService.SimularCenarioCompletoAsync(talhaoId);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao simular cenário completo");
            return StatusCode(500, new { Erros = "Erro ao processar simulação", details = ex.Message });
        }
    }

    /// <summary>
    /// Retorna documentação completa de todos os cenários disponíveis
    /// </summary>
    /// <returns>Objeto JSON com documentação completa de todos os cenários disponíveis</returns>
    /// <response code="200">Documentação retornada com sucesso. Sempre retorna 200 (este endpoint não falha).</response>
    [HttpGet("cenarios")]
    [EndpointSummary("Retorna documentação completa de todos os cenários disponíveis")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> ObterCenariosDisponiveis()
    {
        var cenarios = new
        {
            Seca = new
            {
                Descricao = "Simula condições de seca com umidade do solo baixa",
                Regra = "Umidade < 30% por 24 horas",
                Severidades = new
                {
                    Normal = "Umidade >= 30% (sem alerta)",
                    Media = "Umidade < 30% (alerta médio)",
                    Alta = "Umidade < 20% (alerta alto)",
                    Critica = "Umidade < 15% (alerta crítico)"
                },
                Endpoint = "POST /api/simulador/seca"
            },
            Geada = new
            {
                Descricao = "Simula condições de geada com temperatura muito baixa",
                Regra = "Temperatura < 2°C por 6 horas",
                Severidades = new
                {
                    Normal = "Temperatura >= 2°C (sem alerta)",
                    Media = "Temperatura < 2°C (alerta médio)",
                    Alta = "Temperatura < 1°C (alerta alto)",
                    Critica = "Temperatura < 0°C (alerta crítico - geada)"
                },
                Endpoint = "POST /api/simulador/geada"
            },
            CalorExcessivo = new
            {
                Descricao = "Simula condições de calor extremo",
                Regra = "Temperatura > 35°C por 12 horas",
                Severidades = new
                {
                    Normal = "Temperatura < 35°C (sem alerta)",
                    Media = "Temperatura > 35°C (alerta médio)",
                    Alta = "Temperatura > 40°C (alerta alto)",
                    Critica = "Temperatura > 43°C (alerta crítico)"
                },
                Endpoint = "POST /api/simulador/calor-excessivo"
            },
            ExcessoUmidade = new
            {
                Descricao = "Simula condições de umidade excessiva",
                Regra = "Umidade > 85% por 48 horas",
                Severidades = new
                {
                    Normal = "Umidade < 85% (sem alerta)",
                    Media = "Umidade > 85% (alerta médio)",
                    Alta = "Umidade > 92% (alerta alto)",
                    Critica = "Umidade > 95% (alerta crítico)"
                },
                Endpoint = "POST /api/simulador/excesso-umidade"
            },
            RiscoPraga = new
            {
                Descricao = "Simula condições favoráveis ao desenvolvimento de pragas",
                Regra = "Temperatura entre 20-30°C e Umidade > 70% por 48 horas",
                Severidades = new
                {
                    Normal = "Umidade < 70% (sem alerta)",
                    Media = "Umidade > 70% (alerta médio)",
                    Alta = "Umidade > 80% (alerta alto)",
                    Critica = "Umidade > 85% (alerta crítico)"
                },
                Endpoint = "POST /api/simulador/risco-praga"
            },
            CenarioCompleto = new
            {
                Descricao = "Executa TODOS os cenários em sequência para teste completo do sistema",
                Endpoint = "POST /api/simulador/cenario-completo/{talhaoId}"
            }
        };

        return Ok(cenarios);
    }
}
