using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Events;
using AgroSolutions.ProcessamentoDados.Application.Interfaces;
using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;

namespace AgroSolutions.ProcessamentoDados.Application.Services;

public class ProcessamentoService : IProcessamentoService
{
    private readonly ILeituraProcessadaRepository _repository;
    private readonly IAgregacaoService _agregacaoService;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<ProcessamentoService> _logger;

    public ProcessamentoService(
        ILeituraProcessadaRepository repository,
        IAgregacaoService agregacaoService,
        IRabbitMQPublisher publisher,
        ILogger<ProcessamentoService> logger)
    {
        _repository = repository;
        _agregacaoService = agregacaoService;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task ProcessarLeituraAsync(LeituraRecebidaEvent evento)
    {
        try
        {
            _logger.LogInformation("?? Processando leitura: {DeviceId} - {Valor}{Unidade}", 
                evento.DeviceId, evento.Valor, evento.Unidade);

            // Verifica se já foi processada
            var jaProcessada = await _repository.ObterPorLeituraOrigemIdAsync(evento.Id);
            if (jaProcessada != null)
            {
                _logger.LogWarning("?? Leitura {Id} já foi processada anteriormente", evento.Id);
                return;
            }

            // Cria entidade processada
            var leitura = new LeituraProcessada(
                evento.Id,
                evento.SensorId,
                evento.DeviceId,
                evento.PropriedadeId,
                evento.TipoSensor,
                evento.Valor,
                evento.Unidade,
                evento.TimestampLeitura,
                evento.TimestampRecebimento,
                evento.Qualidade,
                evento.TalhaoId,
                evento.NivelBateria,
                evento.IntensidadeSinal,
                evento.DadosAdicionais
            );

            await _repository.AdicionarAsync(leitura);

            _logger.LogInformation("? Leitura processada com sucesso: {Id}", leitura.Id);

            // Publicar evento para Análise processar regras
            if (evento.TalhaoId.HasValue)
            {
                await _publisher.PublishAsync(new DadosProcessadosEvent(
                    Id: leitura.Id,
                    LeituraOrigemId: leitura.LeituraOrigemId,
                    SensorId: leitura.SensorId,
                    DeviceId: leitura.DeviceId,
                    PropriedadeId: leitura.PropriedadeId,
                    TalhaoId: leitura.TalhaoId,
                    TipoSensor: leitura.TipoSensor,
                    Valor: leitura.Valor,
                    Unidade: leitura.Unidade,
                    TimestampLeitura: leitura.TimestampLeitura,
                    TimestampProcessamento: leitura.TimestampProcessamento,
                    Qualidade: leitura.Qualidade,
                    NivelBateria: leitura.NivelBateria,
                    IntensidadeSinal: leitura.IntensidadeSinal,
                    DadosAdicionais: leitura.DadosAdicionais
                ), "leitura.processada");
            }

            // Gera agregações (hora e dia)
            await GerarAgregacoesAutomaticasAsync(evento.SensorId, evento.TimestampLeitura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro ao processar leitura: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<LeituraProcessadaDto?> ObterPorIdAsync(Guid id)
    {
        var leitura = await _repository.ObterPorIdAsync(id);
        return leitura == null ? null : MapearParaDto(leitura);
    }

    public async Task<LeituraProcessadaDto?> ObterPorLeituraOrigemIdAsync(Guid leituraOrigemId)
    {
        var leitura = await _repository.ObterPorLeituraOrigemIdAsync(leituraOrigemId);
        return leitura == null ? null : MapearParaDto(leitura);
    }

    public async Task<IEnumerable<LeituraProcessadaDto>> ConsultarLeiturasAsync(ConsultarLeiturasDto filtros)
    {
        IEnumerable<LeituraProcessada> leituras;

        if (filtros.SensorId.HasValue)
        {
            leituras = await _repository.ObterPorSensorAsync(
                filtros.SensorId.Value,
                filtros.DataInicio,
                filtros.DataFim
            );
        }
        else if (filtros.PropriedadeId.HasValue)
        {
            leituras = await _repository.ObterPorPropriedadeAsync(
                filtros.PropriedadeId.Value,
                filtros.DataInicio,
                filtros.DataFim
            );
        }
        else if (filtros.TalhaoId.HasValue)
        {
            leituras = await _repository.ObterPorTalhaoAsync(
                filtros.TalhaoId.Value,
                filtros.DataInicio,
                filtros.DataFim
            );
        }
        else
        {
            throw new ArgumentException("É necessário informar SensorId, PropriedadeId ou TalhaoId");
        }

        // Filtros adicionais
        if (filtros.Status.HasValue)
        {
            leituras = leituras.Where(l => l.Status == filtros.Status.Value);
        }

        if (filtros.Qualidade.HasValue)
        {
            leituras = leituras.Where(l => l.Qualidade == filtros.Qualidade.Value);
        }

        // Paginação
        var skip = ((filtros.Pagina ?? 1) - 1) * (filtros.TamanhoPagina ?? 50);
        leituras = leituras.Skip(skip).Take(filtros.TamanhoPagina ?? 50);

        return leituras.Select(MapearParaDto);
    }

    public async Task<EstatisticasProcessamentoDto> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim)
    {
        var totalProcessadas = await _repository.ContarPorStatusAsync(StatusProcessamento.Processado);
        var totalFalhas = await _repository.ContarPorStatusAsync(StatusProcessamento.Falha);
        var total = totalProcessadas + totalFalhas;

        var taxaSucesso = total > 0 ? (decimal)totalProcessadas / total * 100 : 0;

        return new EstatisticasProcessamentoDto(
            dataInicio,
            dataFim,
            total,
            totalProcessadas,
            totalFalhas,
            0, // TODO: Implementar contagem por qualidade
            0,
            0,
            taxaSucesso,
            TimeSpan.Zero, // TODO: Calcular tempo médio
            0 // TODO: Contar agregações
        );
    }

    public async Task ReprocessarFalhasAsync(int limite = 100)
    {
        var falhas = await _repository.ObterComFalhaAsync(limite);

        foreach (var leitura in falhas)
        {
            try
            {
                leitura.Reprocessar();
                leitura.MarcarComoProcessado();
                await _repository.AtualizarAsync(leitura);

                _logger.LogInformation("? Leitura reprocessada: {Id}", leitura.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Erro ao reprocessar leitura {Id}", leitura.Id);
                leitura.MarcarComoFalha(ex.Message);
                await _repository.AtualizarAsync(leitura);
            }
        }
    }

    public async Task<int> ContarPorStatusAsync(string status)
    {
        if (!Enum.TryParse<StatusProcessamento>(status, out var statusEnum))
        {
            throw new ArgumentException($"Status inválido: {status}");
        }

        return await _repository.ContarPorStatusAsync(statusEnum);
    }

    private async Task GerarAgregacoesAutomaticasAsync(Guid sensorId, DateTime timestamp)
    {
        try
        {
            // Gera agregação horária
            var hora = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0, DateTimeKind.Utc);
            if (!await _agregacaoService.AgregacaoExisteAsync(sensorId, "Horaria", hora))
            {
                await _agregacaoService.GerarAgregacaoHorariaAsync(sensorId, hora);
            }

            // Gera agregação diária (se mudou de dia)
            var dia = timestamp.Date;
            if (!await _agregacaoService.AgregacaoExisteAsync(sensorId, "Diaria", dia))
            {
                await _agregacaoService.GerarAgregacaoDiariaAsync(sensorId, dia);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "?? Erro ao gerar agregações automáticas: {Message}", ex.Message);
            // Não lança exceção para não interromper o processamento
        }
    }

    private static LeituraProcessadaDto MapearParaDto(LeituraProcessada leitura)
    {
        return new LeituraProcessadaDto(
            leitura.Id,
            leitura.LeituraOrigemId,
            leitura.SensorId,
            leitura.DeviceId,
            leitura.PropriedadeId,
            leitura.TalhaoId,
            leitura.TipoSensor,
            leitura.Valor,
            leitura.Unidade,
            leitura.TimestampLeitura,
            leitura.TimestampRecebimento,
            leitura.TimestampProcessamento,
            leitura.Qualidade,
            leitura.NivelBateria,
            leitura.IntensidadeSinal,
            leitura.Status,
            leitura.DadosAdicionais,
            leitura.MensagemErro
        );
    }
}
