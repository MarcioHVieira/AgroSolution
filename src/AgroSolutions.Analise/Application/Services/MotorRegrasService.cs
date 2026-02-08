using AgroSolutions.Analise.Application.Interfaces;
using AgroSolutions.Analise.Configuration.Settings;
using AgroSolutions.Analise.Domain.Enums;
using AgroSolutions.Analise.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace AgroSolutions.Analise.Application.Services;

/// <summary>
/// Motor de Regras - Utiliza cache em memória
/// </summary>
public class MotorRegrasService : IMotorRegrasService
{
    private readonly IAlertaService _alertaService;
    private readonly IAlertaRepository _alertaRepository;
    private readonly ILogger<MotorRegrasService> _logger;
    private readonly MotorRegrasSettings _settings;
    private readonly IMemoryCache _cache;

    // Cache em memória das últimas leituras por talhão
    private static readonly ConcurrentDictionary<Guid, List<LeituraCache>> _leiturasCache = new();

    public MotorRegrasService(
        IAlertaService alertaService,
        IAlertaRepository alertaRepository,
        ILogger<MotorRegrasService> logger,
        IOptions<MotorRegrasSettings> settings,
        IMemoryCache cache)
    {
        _alertaService = alertaService;
        _alertaRepository = alertaRepository;
        _logger = logger;
        _settings = settings.Value;
        _cache = cache;
    }

    public async Task ProcessarLeituraEAvaliarRegrasAsync(LeituraParaAnaliseDto leitura)
    {
        _logger.LogInformation("Processando leitura para análise - TalhaoId: {TalhaoId}, Tipo: {Tipo}, Valor: {Valor}",
            leitura.TalhaoId, leitura.TipoSensor, leitura.Valor);

        // 1. Armazenar leitura no cache em memória
        ArmazenarLeituraNoCache(leitura);

        // 2. Avaliar regras aplicáveis
        await AvaliarRegrasParaTalhaoAsync(leitura.TalhaoId);
    }

    private void ArmazenarLeituraNoCache(LeituraParaAnaliseDto leitura)
    {
        var leituraCache = new LeituraCache(
            leitura.TipoSensor,
            leitura.Valor,
            leitura.TimestampLeitura
        );

        var totalLeituras = 0;
        _leiturasCache.AddOrUpdate(
            leitura.TalhaoId,
            _ => new List<LeituraCache> { leituraCache },
            (_, leituras) =>
            {
                leituras.Add(leituraCache);
                
                // Manter apenas leituras das últimas 48 horas a partir da leitura mais recente
                var timestampMaisRecente = leituras.Max(l => l.Timestamp);
                var dataLimite = timestampMaisRecente.AddHours(-48);
                leituras.RemoveAll(l => l.Timestamp < dataLimite);
                
                totalLeituras = leituras.Count;
                return leituras;
            }
        );

        _logger.LogDebug("Leitura armazenada no cache - TalhaoId: {TalhaoId}, TipoSensor: {Tipo}, Total no cache: {Total}, Timestamp: {Timestamp}", 
            leitura.TalhaoId, leitura.TipoSensor, totalLeituras > 0 ? totalLeituras : 1, leitura.TimestampLeitura);
    }

    private async Task AvaliarRegrasParaTalhaoAsync(Guid talhaoId)
    {
        _logger.LogInformation("Avaliando regras para talhão {TalhaoId}", talhaoId);

        try
        {
            if (_settings.RegrasSeca.Habilitada)
                await AvaliarRegraSecaAsync(talhaoId);

            if (_settings.RegrasGeada.Habilitada)
                await AvaliarRegraGeadaAsync(talhaoId);

            if (_settings.RegrasCalorExcessivo.Habilitada)
                await AvaliarRegraCalorExcessivoAsync(talhaoId);

            if (_settings.RegrasExcessoUmidade.Habilitada)
                await AvaliarRegraExcessoUmidadeAsync(talhaoId);

            if (_settings.RegrasRiscoPraga.Habilitada)
                await AvaliarRiscoPragaAsync(talhaoId);

            _logger.LogInformation("Regras avaliadas com sucesso para talhão {TalhaoId}", talhaoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao avaliar regras para talhão {TalhaoId}", talhaoId);
            throw;
        }
    }

    private async Task AvaliarRegraSecaAsync(Guid talhaoId)
    {
        var config = _settings.RegrasSeca;
        var leituras = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 3); // Umidade

        if (!leituras.Any())
        {
            _logger.LogDebug("Nenhuma leitura de umidade encontrada no cache para talhão {TalhaoId}", talhaoId);
            return;
        }

        // Verificar se temos leituras suficientes cobrindo o período necessário
        var primeiraLeitura = leituras.Last();
        var ultimaLeitura = leituras.First();
        var periodoCobertoHoras = (ultimaLeitura.Timestamp - primeiraLeitura.Timestamp).TotalHours;
        
        if (periodoCobertoHoras < config.DuracaoHoras * 0.9) // Aceitar 90% do período
        {
            _logger.LogDebug("Período coberto ({Periodo:F1}h) insuficiente para avaliar regra de seca (necessário: {Necessario}h) - TalhaoId: {TalhaoId}", 
                periodoCobertoHoras, config.DuracaoHoras, talhaoId);
            return;
        }

        _logger.LogInformation("Avaliando regra de SECA - TalhaoId: {TalhaoId}, Leituras: {Count}, Período: {Periodo:F1}h", 
            talhaoId, leituras.Count, periodoCobertoHoras);

        var mediaUmidade = leituras.Average(l => l.Valor);
        var todasAbaixoThreshold = leituras.All(l => l.Valor < config.ThresholdUmidade);



        if (todasAbaixoThreshold)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.Seca);
            if (!jaExisteAlerta)
            {
                var severidade = mediaUmidade switch
                {
                    var u when u < config.SeveridadeCritico => NivelSeveridade.Critico,
                    var u when u < config.SeveridadeAlto => NivelSeveridade.Alto,
                    _ => NivelSeveridade.Medio
                };

                await _alertaService.CriarAsync(new DTOs.CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.Seca,
                    Severidade: severidade,
                    Titulo: "Alerta de Seca",
                    Mensagem: $"Umidade do solo abaixo de {config.ThresholdUmidade}% por mais de {config.DuracaoHoras} horas. Média: {mediaUmidade:F1}%",
                    Recomendacao: "Recomenda-se irrigação imediata. Verifique o sistema de irrigação.",
                    ValorReferencia: mediaUmidade
                ));

                _logger.LogWarning("Alerta de SECA gerado - TalhaoId: {TalhaoId}, Umidade: {Umidade}%", talhaoId, mediaUmidade);
            }
        }
    }

    private async Task AvaliarRegraGeadaAsync(Guid talhaoId)
    {
        var config = _settings.RegrasGeada;
        var leituras = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 1); // Temperatura

        if (!leituras.Any())
            return;

        // Verificar se temos leituras suficientes cobrindo o período necessário
        var primeiraLeitura = leituras.Last();
        var ultimaLeitura = leituras.First();
        var periodoCobertoHoras = (ultimaLeitura.Timestamp - primeiraLeitura.Timestamp).TotalHours;
        
        if (periodoCobertoHoras < config.DuracaoHoras * 0.8)
        {
            _logger.LogDebug("Período coberto ({Periodo:F1}h) insuficiente para avaliar regra de geada - TalhaoId: {TalhaoId}", 
                periodoCobertoHoras, talhaoId);
            return;
        }

        _logger.LogInformation("Avaliando regra de GEADA - TalhaoId: {TalhaoId}, Leituras: {Count}, Período: {Periodo:F1}h", 
            talhaoId, leituras.Count, periodoCobertoHoras);

        var temperaturaMinima = leituras.Min(l => l.Valor);

        if (temperaturaMinima < config.ThresholdTemperatura)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.Geada);
            if (!jaExisteAlerta)
            {
                var severidade = temperaturaMinima switch
                {
                    var t when t < config.SeveridadeCritico => NivelSeveridade.Critico,
                    var t when t < config.SeveridadeAlto => NivelSeveridade.Alto,
                    _ => NivelSeveridade.Medio
                };

                await _alertaService.CriarAsync(new DTOs.CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.Geada,
                    Severidade: severidade,
                    Titulo: "Risco de Geada",
                    Mensagem: $"Temperatura abaixo de {config.ThresholdTemperatura}°C. Mínima: {temperaturaMinima:F1}°C",
                    Recomendacao: "Ativar sistema anti-geada se disponível.",
                    ValorReferencia: temperaturaMinima
                ));

                _logger.LogWarning("Alerta de GEADA gerado - TalhaoId: {TalhaoId}, Temp: {Temp}°C", talhaoId, temperaturaMinima);
            }
        }
    }

    private async Task AvaliarRegraCalorExcessivoAsync(Guid talhaoId)
    {
        var config = _settings.RegrasCalorExcessivo;
        var leituras = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 1); // Temperatura

        if (!leituras.Any())
            return;

        // Verificar se temos leituras suficientes cobrindo o período necessário
        var primeiraLeitura = leituras.Last();
        var ultimaLeitura = leituras.First();
        var periodoCobertoHoras = (ultimaLeitura.Timestamp - primeiraLeitura.Timestamp).TotalHours;
        
        if (periodoCobertoHoras < config.DuracaoHoras * 0.8)
        {
            _logger.LogDebug("Período coberto ({Periodo:F1}h) insuficiente para avaliar regra de calor - TalhaoId: {TalhaoId}", 
                periodoCobertoHoras, talhaoId);
            return;
        }

        _logger.LogInformation("Avaliando regra de CALOR EXCESSIVO - TalhaoId: {TalhaoId}, Leituras: {Count}, Período: {Periodo:F1}h", 
            talhaoId, leituras.Count, periodoCobertoHoras);

        var temperaturaMaxima = leituras.Max(l => l.Valor);
        var todasAcimaThreshold = leituras.All(l => l.Valor >= config.ThresholdTemperatura);

        if (todasAcimaThreshold)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.CalorExcessivo);
            if (!jaExisteAlerta)
            {
                var severidade = temperaturaMaxima switch
                {
                    var t when t > config.SeveridadeCritico => NivelSeveridade.Critico,
                    var t when t > config.SeveridadeAlto => NivelSeveridade.Alto,
                    _ => NivelSeveridade.Medio
                };

                await _alertaService.CriarAsync(new DTOs.CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.CalorExcessivo,
                    Severidade: severidade,
                    Titulo: "Calor Excessivo",
                    Mensagem: $"Temperatura acima de {config.ThresholdTemperatura}°C por {config.DuracaoHoras}h. Máxima: {temperaturaMaxima:F1}°C",
                    Recomendacao: "Aumentar frequência de irrigação.",
                    ValorReferencia: temperaturaMaxima
                ));

                _logger.LogWarning("Alerta de CALOR gerado - TalhaoId: {TalhaoId}, Temp: {Temp}°C", talhaoId, temperaturaMaxima);
            }
        }
    }

    private async Task AvaliarRegraExcessoUmidadeAsync(Guid talhaoId)
    {
        var config = _settings.RegrasExcessoUmidade;
        var leituras = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 3); // Umidade

        if (!leituras.Any())
            return;

        // Verificar se temos leituras suficientes cobrindo o período necessário
        var primeiraLeitura = leituras.Last();
        var ultimaLeitura = leituras.First();
        var periodoCobertoHoras = (ultimaLeitura.Timestamp - primeiraLeitura.Timestamp).TotalHours;
        
        if (periodoCobertoHoras < config.DuracaoHoras * 0.8)
        {
            _logger.LogDebug("Período coberto ({Periodo:F1}h) insuficiente para avaliar regra de excesso de umidade - TalhaoId: {TalhaoId}", 
                periodoCobertoHoras, talhaoId);
            return;
        }

        _logger.LogInformation("Avaliando regra de EXCESSO DE UMIDADE - TalhaoId: {TalhaoId}, Leituras: {Count}, Período: {Periodo:F1}h", 
            talhaoId, leituras.Count, periodoCobertoHoras);

        var mediaUmidade = leituras.Average(l => l.Valor);
        var todasAcimaThreshold = leituras.All(l => l.Valor >= config.ThresholdUmidade);

        if (todasAcimaThreshold)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.ExcessoUmidade);
            if (!jaExisteAlerta)
            {
                var severidade = mediaUmidade > config.SeveridadeAlto ? NivelSeveridade.Alto : NivelSeveridade.Medio;

                await _alertaService.CriarAsync(new DTOs.CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.ExcessoUmidade,
                    Severidade: severidade,
                    Titulo: "Excesso de Umidade",
                    Mensagem: $"Umidade acima de {config.ThresholdUmidade}% por {config.DuracaoHoras}h. Média: {mediaUmidade:F1}%",
                    Recomendacao: "Verificar drenagem. Suspender irrigação temporariamente.",
                    ValorReferencia: mediaUmidade
                ));

                _logger.LogWarning("Alerta de EXCESSO DE UMIDADE gerado - TalhaoId: {TalhaoId}, Umidade: {Umidade}%", talhaoId, mediaUmidade);
            }
        }
    }

    private async Task AvaliarRiscoPragaAsync(Guid talhaoId)
    {
        var config = _settings.RegrasRiscoPraga;
        var leiturasTemp = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 1);
        var leiturasUmid = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 3);

        if (!leiturasTemp.Any() || !leiturasUmid.Any())
            return;

        // Verificar se temos leituras suficientes cobrindo o período necessário
        var periodoCobertoTemp = (leiturasTemp.First().Timestamp - leiturasTemp.Last().Timestamp).TotalHours;
        var periodoCobertoUmid = (leiturasUmid.First().Timestamp - leiturasUmid.Last().Timestamp).TotalHours;
        
        if (periodoCobertoTemp < config.DuracaoHoras * 0.9 || periodoCobertoUmid < config.DuracaoHoras * 0.9)
        {
            _logger.LogDebug("Período coberto insuficiente para avaliar regra de risco de praga - TalhaoId: {TalhaoId}", talhaoId);
            return;
        }

        _logger.LogInformation("Avaliando regra de RISCO DE PRAGA - TalhaoId: {TalhaoId}, Leituras Temp: {TempCount}, Umid: {UmidCount}", 
            talhaoId, leiturasTemp.Count, leiturasUmid.Count);

        // Verificar condições propícias para pragas
        var condicoesPropicias = leiturasTemp.Count(temp =>
        {
            var tempValor = temp.Valor;
            var umidadeNoMesmoMomento = leiturasUmid
                .Where(u => Math.Abs((u.Timestamp - temp.Timestamp).TotalMinutes) < 60)
                .Select(u => u.Valor)
                .FirstOrDefault();

            return tempValor >= config.TemperaturaMinima &&
                   tempValor <= config.TemperaturaMaxima &&
                   umidadeNoMesmoMomento > config.ThresholdUmidade;
        });

        var percentual = (decimal)condicoesPropicias / leiturasTemp.Count * 100;

        if (percentual >= config.PercentualCondicoes)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.RiscoPraga);
            if (!jaExisteAlerta)
            {
                await _alertaService.CriarAsync(new DTOs.CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.RiscoPraga,
                    Severidade: NivelSeveridade.Medio,
                    Titulo: "Risco de Praga",
                    Mensagem: $"Condições favoráveis para pragas ({percentual:F0}% do tempo)",
                    Recomendacao: "Realizar vistoria preventiva.",
                    ValorReferencia: percentual
                ));

                _logger.LogWarning("Alerta de RISCO DE PRAGA gerado - TalhaoId: {TalhaoId}", talhaoId);
            }
        }
    }

    private List<LeituraCache> ObterLeiturasDoCache(Guid talhaoId, int ultimasHoras, int tipoSensor)
    {
        if (!_leiturasCache.TryGetValue(talhaoId, out var todasLeituras))
            return new List<LeituraCache>();

        var leiturasMesmoTipo = todasLeituras
            .Where(l => l.TipoSensor == tipoSensor)
            .OrderByDescending(l => l.Timestamp)
            .ToList();

        if (!leiturasMesmoTipo.Any())
            return new List<LeituraCache>();

        // Usar o timestamp da leitura mais recente como referência
        var timestampMaisRecente = leiturasMesmoTipo.First().Timestamp;
        var dataLimite = timestampMaisRecente.AddHours(-ultimasHoras);

        return leiturasMesmoTipo
            .Where(l => l.Timestamp >= dataLimite)
            .ToList();
    }

    private record LeituraCache(int TipoSensor, decimal Valor, DateTime Timestamp);
}
