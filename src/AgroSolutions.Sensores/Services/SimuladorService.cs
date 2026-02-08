using AgroSolutions.Sensores.Configuration;
using AgroSolutions.Sensores.Models;
using Microsoft.Extensions.Options;

namespace AgroSolutions.Sensores.Services;

public class SimuladorService : ISimuladorService
{
    private readonly IIngestaoApiClient _ingestaoClient;
    private readonly SimuladorSettings _settings;
    private readonly ILogger<SimuladorService> _logger;
    private readonly Random _random;

    public SimuladorService(
        IIngestaoApiClient ingestaoClient,
        IOptions<SimuladorSettings> settings,
        ILogger<SimuladorService> logger)
    {
        _ingestaoClient = ingestaoClient;
        _settings = settings.Value;
        _logger = logger;
        _random = new Random();
    }

    public async Task<ResultadoSimulacaoDto> SimularSecaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
    {
        _logger.LogInformation("Iniciando simulação de SECA - Severidade: {Severidade} - TalhaoId: {TalhaoId}", 
            severidade, talhaoId);

        var config = _settings.RegrasSeca;
        var umidadeBase = severidade switch
        {
            Severidade.Normal => config.ThresholdNormal + 5.0m,
            Severidade.Media => (config.ThresholdMedia + config.ThresholdNormal) / 2,
            Severidade.Alta => (config.ThresholdAlta + config.ThresholdMedia) / 2,
            Severidade.Critica => config.ThresholdCritica - 2.0m,
            _ => config.ThresholdNormal
        };

        var leituras = new List<LeituraSimuladaDto>();
        var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
        var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);

        for (int i = 0; i < totalLeituras; i++)
        {
            var valor = AplicarVariacao(umidadeBase, _settings.Simulacao.VariacaoAleatoria);
            valor = Math.Max(0, Math.Min(100, valor)); // Garantir entre 0-100%

            var leitura = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.UmidadeSolo.ToString(),
                Valor = Math.Round(valor, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leitura);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leitura);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }
        }

        var resultado = CriarResultado("SECA", severidade.ToString(), leituras);
        resultado = resultado with
        {
            Mensagem = $"Simulação de SECA concluída. {leituras.Count} leituras geradas com umidade média de {resultado.ValorMedio:F2}%"
        };

        _logger.LogInformation("Simulação de SECA concluída - {Quantidade} leituras enviadas", leituras.Count);
        return resultado;
    }

    public async Task<ResultadoSimulacaoDto> SimularGeadaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
    {
        _logger.LogInformation("Iniciando simulação de GEADA - Severidade: {Severidade} - TalhaoId: {TalhaoId}", 
            severidade, talhaoId);

        var config = _settings.RegrasGeada;
        var temperaturaBase = severidade switch
        {
            Severidade.Normal => config.ThresholdNormal + 3.0m,
            Severidade.Media => (config.ThresholdMedia + config.ThresholdNormal) / 2,
            Severidade.Alta => (config.ThresholdAlta + config.ThresholdMedia) / 2,
            Severidade.Critica => config.ThresholdCritica - 2.0m,
            _ => config.ThresholdNormal
        };

        var leituras = new List<LeituraSimuladaDto>();
        var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
        var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);

        for (int i = 0; i < totalLeituras; i++)
        {
            var valor = AplicarVariacao(temperaturaBase, _settings.Simulacao.VariacaoAleatoria);
            valor = Math.Max(-10, Math.Min(50, valor)); // Garantir entre -10 e 50°C

            var leitura = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.Temperatura.ToString(),
                Valor = Math.Round(valor, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leitura);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leitura);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }
        }

        var resultado = CriarResultado("GEADA", severidade.ToString(), leituras);
        resultado = resultado with
        {
            Mensagem = $"Simulação de GEADA concluída. {leituras.Count} leituras geradas com temperatura média de {resultado.ValorMedio:F2}°C"
        };

        _logger.LogInformation("Simulação de GEADA concluída - {Quantidade} leituras enviadas", leituras.Count);
        return resultado;
    }

    public async Task<ResultadoSimulacaoDto> SimularCalorExcessivoAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
    {
        _logger.LogInformation("Iniciando simulação de CALOR EXCESSIVO - Severidade: {Severidade} - TalhaoId: {TalhaoId}", 
            severidade, talhaoId);

        var config = _settings.RegrasCalorExcessivo;
        var temperaturaBase = severidade switch
        {
            Severidade.Normal => config.ThresholdNormal - 2.0m,
            Severidade.Media => (config.ThresholdMedia + config.ThresholdNormal) / 2,
            Severidade.Alta => (config.ThresholdAlta + config.ThresholdMedia) / 2,
            Severidade.Critica => config.ThresholdCritica + 2.0m,
            _ => config.ThresholdNormal
        };

        var leituras = new List<LeituraSimuladaDto>();
        var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
        var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);

        for (int i = 0; i < totalLeituras; i++)
        {
            var valor = AplicarVariacao(temperaturaBase, _settings.Simulacao.VariacaoAleatoria);
            valor = Math.Max(0, Math.Min(60, valor)); // Garantir entre 0 e 60°C

            var leitura = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.Temperatura.ToString(),
                Valor = Math.Round(valor, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leitura);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leitura);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }
        }

        var resultado = CriarResultado("CALOR_EXCESSIVO", severidade.ToString(), leituras);
        resultado = resultado with
        {
            Mensagem = $"Simulação de CALOR EXCESSIVO concluída. {leituras.Count} leituras geradas com temperatura média de {resultado.ValorMedio:F2}°C"
        };

        _logger.LogInformation("Simulação de CALOR EXCESSIVO concluída - {Quantidade} leituras enviadas", leituras.Count);
        return resultado;
    }

    public async Task<ResultadoSimulacaoDto> SimularExcessoUmidadeAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
    {
        _logger.LogInformation("Iniciando simulação de EXCESSO DE UMIDADE - Severidade: {Severidade} - TalhaoId: {TalhaoId}", 
            severidade, talhaoId);

        var config = _settings.RegrasExcessoUmidade;
        var umidadeBase = severidade switch
        {
            Severidade.Normal => config.ThresholdNormal - 3.0m,
            Severidade.Media => (config.ThresholdMedia + config.ThresholdNormal) / 2,
            Severidade.Alta => (config.ThresholdAlta + config.ThresholdMedia) / 2,
            Severidade.Critica => config.ThresholdCritica + 2.0m,
            _ => config.ThresholdNormal
        };

        var leituras = new List<LeituraSimuladaDto>();
        var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
        var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);

        for (int i = 0; i < totalLeituras; i++)
        {
            var valor = AplicarVariacao(umidadeBase, _settings.Simulacao.VariacaoAleatoria);
            valor = Math.Max(0, Math.Min(100, valor)); // Garantir entre 0-100%

            var leitura = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.UmidadeSolo.ToString(),
                Valor = Math.Round(valor, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leitura);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leitura);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }
        }

        var resultado = CriarResultado("EXCESSO_UMIDADE", severidade.ToString(), leituras);
        resultado = resultado with
        {
            Mensagem = $"Simulação de EXCESSO DE UMIDADE concluída. {leituras.Count} leituras geradas com umidade média de {resultado.ValorMedio:F2}%"
        };

        _logger.LogInformation("Simulação de EXCESSO DE UMIDADE concluída - {Quantidade} leituras enviadas", leituras.Count);
        return resultado;
    }

    public async Task<ResultadoSimulacaoDto> SimularRiscoPragaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true)
    {
        _logger.LogInformation("Iniciando simulação de RISCO DE PRAGA - Severidade: {Severidade} - TalhaoId: {TalhaoId}", 
            severidade, talhaoId);

        var config = _settings.RegrasRiscoPraga;
        
        // Temperatura entre 20-30°C
        var temperaturaMedia = (config.TemperaturaMin + config.TemperaturaMax) / 2;
        
        // Umidade acima de 70%
        var umidadeBase = severidade switch
        {
            Severidade.Normal => config.ThresholdUmidade - 5.0m,
            Severidade.Media => config.ThresholdUmidade + 5.0m,
            Severidade.Alta => config.ThresholdUmidade + 10.0m,
            Severidade.Critica => config.ThresholdUmidade + 15.0m,
            _ => config.ThresholdUmidade
        };

        var leituras = new List<LeituraSimuladaDto>();
        var totalLeituras = (config.DuracaoHoras * 60) / config.IntervaloMinutos;
        var dataInicio = DateTime.UtcNow.AddHours(-config.DuracaoHoras);

        for (int i = 0; i < totalLeituras; i++)
        {
            // Gerar leitura de temperatura
            var temperatura = AplicarVariacao(temperaturaMedia, 3.0m);
            temperatura = Math.Max(config.TemperaturaMin, Math.Min(config.TemperaturaMax, temperatura));

            var leituraTemp = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.Temperatura.ToString(),
                Valor = Math.Round(temperatura, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leituraTemp);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leituraTemp);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }

            // Gerar leitura de umidade
            var umidade = AplicarVariacao(umidadeBase, _settings.Simulacao.VariacaoAleatoria);
            umidade = Math.Max(0, Math.Min(100, umidade));

            var leituraUmid = new LeituraSimuladaDto
            {
                TalhaoId = talhaoId,
                TipoSensor = TipoSensor.UmidadeSolo.ToString(),
                Valor = Math.Round(umidade, 2),
                DataHora = dataInicio.AddMinutes(i * config.IntervaloMinutos)
            };

            leituras.Add(leituraUmid);

            if (enviarParaApi)
            {
                await _ingestaoClient.EnviarLeituraAsync(leituraUmid);
                await Task.Delay(_settings.Simulacao.DelayEntreLeiturasMs);
            }
        }

        var resultado = CriarResultado("RISCO_PRAGA", severidade.ToString(), leituras);
        resultado = resultado with
        {
            Mensagem = $"Simulação de RISCO DE PRAGA concluída. {leituras.Count} leituras geradas (temperatura e umidade)"
        };

        _logger.LogInformation("Simulação de RISCO DE PRAGA concluída - {Quantidade} leituras enviadas", leituras.Count);
        return resultado;
    }

    public async Task<ResultadoSimulacaoDto> SimularCenarioCompletoAsync(Guid talhaoId)
    {
        _logger.LogInformation("Iniciando simulação de CENÁRIO COMPLETO - TalhaoId: {TalhaoId}", talhaoId);

        var todasLeituras = new List<LeituraSimuladaDto>();
        var inicioTotal = DateTime.UtcNow;

        // Simular todos os cenários em sequência
        var resultado1 = await SimularSecaAsync(talhaoId, Severidade.Critica, true);
        todasLeituras.AddRange(resultado1.Leituras);
        await Task.Delay(1000);

        var resultado2 = await SimularGeadaAsync(talhaoId, Severidade.Alta, true);
        todasLeituras.AddRange(resultado2.Leituras);
        await Task.Delay(1000);

        var resultado3 = await SimularCalorExcessivoAsync(talhaoId, Severidade.Media, true);
        todasLeituras.AddRange(resultado3.Leituras);
        await Task.Delay(1000);

        var resultado4 = await SimularExcessoUmidadeAsync(talhaoId, Severidade.Media, true);
        todasLeituras.AddRange(resultado4.Leituras);
        await Task.Delay(1000);

        var resultado5 = await SimularRiscoPragaAsync(talhaoId, Severidade.Alta, true);
        todasLeituras.AddRange(resultado5.Leituras);

        var fimTotal = DateTime.UtcNow;

        var resultadoCompleto = new ResultadoSimulacaoDto
        {
            Cenario = "CENARIO_COMPLETO",
            Severidade = "Misto",
            QuantidadeLeituras = todasLeituras.Count,
            InicioSimulacao = inicioTotal,
            FimSimulacao = fimTotal,
            ValorMedio = todasLeituras.Any() ? todasLeituras.Average(l => l.Valor) : 0,
            ValorMinimo = todasLeituras.Any() ? todasLeituras.Min(l => l.Valor) : 0,
            ValorMaximo = todasLeituras.Any() ? todasLeituras.Max(l => l.Valor) : 0,
            Leituras = todasLeituras,
            Mensagem = $"Cenário completo simulado com sucesso. Total de {todasLeituras.Count} leituras geradas abrangendo todos os tipos de alertas."
        };

        _logger.LogInformation("Simulação de CENÁRIO COMPLETO concluída - {Quantidade} leituras totais enviadas", todasLeituras.Count);
        return resultadoCompleto;
    }

    private decimal AplicarVariacao(decimal valorBase, decimal variacaoMaxima)
    {
        if (!_settings.Simulacao.AdicionarRuido)
            return valorBase;

        var variacao = (decimal)(_random.NextDouble() * 2.0 - 1.0) * variacaoMaxima;
        return valorBase + variacao;
    }

    private ResultadoSimulacaoDto CriarResultado(string cenario, string severidade, List<LeituraSimuladaDto> leituras)
    {
        return new ResultadoSimulacaoDto
        {
            Cenario = cenario,
            Severidade = severidade,
            QuantidadeLeituras = leituras.Count,
            InicioSimulacao = leituras.Any() ? leituras.Min(l => l.DataHora) : DateTime.UtcNow,
            FimSimulacao = leituras.Any() ? leituras.Max(l => l.DataHora) : DateTime.UtcNow,
            ValorMedio = leituras.Any() ? Math.Round(leituras.Average(l => l.Valor), 2) : 0,
            ValorMinimo = leituras.Any() ? leituras.Min(l => l.Valor) : 0,
            ValorMaximo = leituras.Any() ? leituras.Max(l => l.Valor) : 0,
            Leituras = leituras
        };
    }
}
