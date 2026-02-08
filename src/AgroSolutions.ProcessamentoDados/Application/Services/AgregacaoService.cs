using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;

namespace AgroSolutions.ProcessamentoDados.Application.Services;

public class AgregacaoService : IAgregacaoService
{
    private readonly IAgregacaoDadosRepository _agregacaoRepository;
    private readonly ILeituraProcessadaRepository _leituraRepository;
    private readonly ILogger<AgregacaoService> _logger;

    public AgregacaoService(
        IAgregacaoDadosRepository agregacaoRepository,
        ILeituraProcessadaRepository leituraRepository,
        ILogger<AgregacaoService> logger)
    {
        _agregacaoRepository = agregacaoRepository;
        _leituraRepository = leituraRepository;
        _logger = logger;
    }

    public async Task GerarAgregacaoHorariaAsync(Guid sensorId, DateTime hora)
    {
        var periodoInicio = new DateTime(hora.Year, hora.Month, hora.Day, hora.Hour, 0, 0, DateTimeKind.Utc);
        var periodoFim = periodoInicio.AddHours(1);

        await GerarAgregacaoAsync(sensorId, TipoAgregacao.Horaria, periodoInicio, periodoFim);
    }

    public async Task GerarAgregacaoDiariaAsync(Guid sensorId, DateTime dia)
    {
        var periodoInicio = dia.Date;
        var periodoFim = periodoInicio.AddDays(1);

        await GerarAgregacaoAsync(sensorId, TipoAgregacao.Diaria, periodoInicio, periodoFim);
    }

    public async Task GerarAgregacaoSemanalAsync(Guid sensorId, DateTime semana)
    {
        var periodoInicio = semana.Date.AddDays(-(int)semana.DayOfWeek);
        var periodoFim = periodoInicio.AddDays(7);

        await GerarAgregacaoAsync(sensorId, TipoAgregacao.Semanal, periodoInicio, periodoFim);
    }

    public async Task GerarAgregacaoMensalAsync(Guid sensorId, DateTime mes)
    {
        var periodoInicio = new DateTime(mes.Year, mes.Month, 1);
        var periodoFim = periodoInicio.AddMonths(1);

        await GerarAgregacaoAsync(sensorId, TipoAgregacao.Mensal, periodoInicio, periodoFim);
    }

    public async Task<AgregacaoDadosDto?> ObterPorIdAsync(Guid id)
    {
        var agregacao = await _agregacaoRepository.ObterPorIdAsync(id);
        return agregacao == null ? null : MapearParaDto(agregacao);
    }

    public async Task<IEnumerable<AgregacaoDadosDto>> ConsultarAgregacoesAsync(ConsultarAgregacoesDto filtros)
    {
        IEnumerable<AgregacaoDados> agregacoes;

        if (filtros.SensorId.HasValue && filtros.TipoAgregacao.HasValue)
        {
            agregacoes = await _agregacaoRepository.ObterPorSensorAsync(
                filtros.SensorId.Value,
                filtros.TipoAgregacao.Value,
                filtros.DataInicio,
                filtros.DataFim
            );
        }
        else if (filtros.PropriedadeId.HasValue && filtros.TipoAgregacao.HasValue)
        {
            agregacoes = await _agregacaoRepository.ObterPorPropriedadeAsync(
                filtros.PropriedadeId.Value,
                filtros.TipoAgregacao.Value,
                filtros.DataInicio,
                filtros.DataFim
            );
        }
        else
        {
            throw new ArgumentException("É necessário informar SensorId ou PropriedadeId junto com TipoAgregacao");
        }

        return agregacoes.Select(MapearParaDto);
    }

    public async Task<bool> AgregacaoExisteAsync(Guid sensorId, string tipoAgregacao, DateTime periodo)
    {
        if (!Enum.TryParse<TipoAgregacao>(tipoAgregacao, out var tipo))
        {
            return false;
        }

        var agregacao = await _agregacaoRepository.ObterPorPeriodoAsync(sensorId, tipo, periodo);
        return agregacao != null;
    }

    private async Task GerarAgregacaoAsync(
        Guid sensorId,
        TipoAgregacao tipo,
        DateTime periodoInicio,
        DateTime periodoFim)
    {
        try
        {
            _logger.LogInformation("?? Gerando agregação {Tipo} para sensor {SensorId}: {Periodo}",
                tipo, sensorId, periodoInicio);

            // Verifica se já existe
            var existe = await _agregacaoRepository.ObterPorPeriodoAsync(sensorId, tipo, periodoInicio);
            if (existe != null)
            {
                _logger.LogWarning("?? Agregação {Tipo} já existe para {Periodo}", tipo, periodoInicio);
                return;
            }

            // Obtém leituras do período
            var leituras = await _leituraRepository.ObterPorSensorAsync(sensorId, periodoInicio, periodoFim);
            var listaLeituras = leituras.ToList();

            if (!listaLeituras.Any())
            {
                _logger.LogWarning("?? Nenhuma leitura encontrada para agregação {Tipo}: {Periodo}", tipo, periodoInicio);
                return;
            }

            // Pega informações da primeira leitura
            var primeiraLeitura = listaLeituras.First();

            // Calcula estatísticas
            var valores = listaLeituras.Select(l => l.Valor).ToList();
            var valorMinimo = valores.Min();
            var valorMaximo = valores.Max();
            var valorMedio = valores.Average();
            
            // Calcula desvio padrão
            var variancia = valores.Select(v => Math.Pow((double)(v - valorMedio), 2)).Average();
            var desvioPadrao = (decimal)Math.Sqrt(variancia);

            // Contadores por qualidade
            var leiturasNormais = listaLeituras.Count(l => l.Qualidade == QualidadeLeitura.Normal);
            var leiturasSuspeitas = listaLeituras.Count(l => l.Qualidade == QualidadeLeitura.Suspeita);
            var leiturasInvalidas = listaLeituras.Count(l => l.Qualidade == QualidadeLeitura.Invalida);

            // Cria agregação
            var agregacao = new AgregacaoDados(
                sensorId,
                primeiraLeitura.DeviceId,
                primeiraLeitura.PropriedadeId,
                primeiraLeitura.TipoSensor,
                tipo,
                periodoInicio,
                periodoFim,
                listaLeituras.Count,
                primeiraLeitura.Unidade,
                primeiraLeitura.TalhaoId,
                valorMinimo,
                valorMaximo,
                valorMedio,
                desvioPadrao,
                leiturasNormais,
                leiturasSuspeitas,
                leiturasInvalidas
            );

            await _agregacaoRepository.AdicionarAsync(agregacao);

            _logger.LogInformation("? Agregação {Tipo} gerada com sucesso: {TotalLeituras} leituras",
                tipo, listaLeituras.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro ao gerar agregação {Tipo}: {Message}", tipo, ex.Message);
            throw;
        }
    }

    private static AgregacaoDadosDto MapearParaDto(AgregacaoDados agregacao)
    {
        return new AgregacaoDadosDto(
            agregacao.Id,
            agregacao.SensorId,
            agregacao.DeviceId,
            agregacao.PropriedadeId,
            agregacao.TalhaoId,
            agregacao.TipoSensor,
            agregacao.TipoAgregacao,
            agregacao.PeriodoInicio,
            agregacao.PeriodoFim,
            agregacao.TotalLeituras,
            agregacao.ValorMinimo,
            agregacao.ValorMaximo,
            agregacao.ValorMedio,
            agregacao.DesvioPadrao,
            agregacao.Unidade,
            agregacao.LeiturasNormais,
            agregacao.LeiturasSuspeitas,
            agregacao.LeiturasInvalidas
        );
    }
}
