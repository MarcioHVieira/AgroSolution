using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Events;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using AgroSolutions.IngestaoDados.Infrastructure.Metrics;

namespace AgroSolutions.IngestaoDados.Application.Services;

public class LeituraService : ILeituraService
{
    private readonly ILeituraSensorRepository _leituraRepository;
    private readonly ISensorRepository _sensorRepository;
    private readonly IMensageriaService _mensageriaService;
    private readonly ILogger<LeituraService> _logger;

    public LeituraService(
        ILeituraSensorRepository leituraRepository,
        ISensorRepository sensorRepository,
        IMensageriaService mensageriaService,
        ILogger<LeituraService> logger)
    {
        _leituraRepository = leituraRepository;
        _sensorRepository = sensorRepository;
        _mensageriaService = mensageriaService;
        _logger = logger;
    }

    public async Task<LeituraSensorDto> RegistrarLeituraAsync(RegistrarLeituraDto dto, CancellationToken cancellationToken = default)
    {
        Sensor? sensor = null;
        
        // Busca o sensor pelo DeviceId
        try
        {
            sensor = await _sensorRepository.ObterPorDeviceIdAsync(dto.DeviceId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar sensor {DeviceId}, tentando criar se for simulador", dto.DeviceId);
        }
        
        // Se o sensor não existe e é do simulador (SIM-), cria automaticamente
        if (sensor == null && dto.DeviceId.StartsWith("SIM-", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Sensor simulado {DeviceId} não encontrado. Criando automaticamente...", dto.DeviceId);
            
            Guid? talhaoId = null;
            var tipoSensorStr = "Desconhecido";
            
            // Remove o prefixo "SIM-"
            var semPrefixo = dto.DeviceId.Substring(4);
            
            // O último segmento após o último hífen é o tipo de sensor
            var ultimoHifen = semPrefixo.LastIndexOf('-');
            if (ultimoHifen > 0)
            {
                tipoSensorStr = semPrefixo.Substring(ultimoHifen + 1);
                var guidStr = semPrefixo.Substring(0, ultimoHifen);
                
                if (Guid.TryParse(guidStr, out var parsedGuid))
                {
                    talhaoId = parsedGuid;
                }
            }
            
            _logger.LogInformation("Parse do DeviceId: TalhaoId={TalhaoId}, TipoSensor={TipoSensor}", 
                talhaoId, tipoSensorStr);
            
            // Determinar tipo de sensor baseado no nome (case-insensitive)
            var tipoSensor = tipoSensorStr.ToUpperInvariant() switch
            {
                "UMIDADESOLO" => TipoSensor.UmidadeSolo,
                "TEMPERATURA" => TipoSensor.Temperatura,
                "PRECIPITACAO" or "PLUVIOMETRO" => TipoSensor.Precipitacao,
                "UMIDADEAR" => TipoSensor.UmidadeAr,
                "PHSOLO" or "PH" => TipoSensor.PHSolo,
                "VELOCIDADEVENTO" => TipoSensor.VelocidadeVento,
                "DIRECAOVENTO" => TipoSensor.DirecaoVento,
                "PRESSAOATMOSFERICA" or "PRESSAO" => TipoSensor.PressaoAtmosferica,
                "CONDUTIVIDADESOLO" or "CONDUTIVIDADE" => TipoSensor.CondutividadeSolo,
                "RADIACAOSOLAR" => TipoSensor.RadiacaoSolar,
                _ => TipoSensor.UmidadeSolo // Default fallback
            };
            
            // Criar sensor simulado temporário (sem PropriedadeId real)
            sensor = new Sensor(
                propriedadeId: Guid.Empty, // Temporário - simulação
                deviceId: dto.DeviceId,
                nome: $"Sensor Simulado - {tipoSensorStr}",
                tipo: tipoSensor,
                intervaloLeituraMinutos: 60,
                talhaoId: talhaoId,
                fabricante: "Simulador",
                modelo: "SIM-v1.0"
            );
            
            try
            {
                await _sensorRepository.AdicionarAsync(sensor, cancellationToken);
                _logger.LogInformation("Sensor simulado {DeviceId} criado com TalhaoId={TalhaoId}", 
                    dto.DeviceId, talhaoId);
            }
            catch (Exception ex)
            {
                // Se já existe (race condition), tenta buscar novamente
                _logger.LogWarning(ex, "Erro ao criar sensor {DeviceId}, pode já existir. Tentando buscar novamente...", dto.DeviceId);
                
                try
                {
                    sensor = await _sensorRepository.ObterPorDeviceIdAsync(dto.DeviceId, cancellationToken);
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Falha ao buscar sensor após erro de criação");
                }
                
                if (sensor == null)
                {
                    throw new InvalidOperationException($"Falha ao criar ou buscar sensor {dto.DeviceId}", ex);
                }
            }
        }
        else if (sensor == null)
        {
            throw new KeyNotFoundException($"Sensor com DeviceId '{dto.DeviceId}' não encontrado");
        }

        if (!sensor.EstaAtivo())
            throw new InvalidOperationException($"Sensor {dto.DeviceId} não está ativo (Status: {sensor.Status})");

        // Valida a leitura
        var qualidade = ValidarLeitura(sensor, dto.Valor);

        // Cria a leitura
        var leitura = new LeituraSensor(
            sensor.Id,
            dto.Valor,
            dto.Unidade,
            dto.TimestampLeitura,
            qualidade,
            dto.NivelBateria,
            dto.IntensidadeSinal,
            dto.DadosAdicionais
        );

        await _leituraRepository.AdicionarAsync(leitura, cancellationToken);
        
        _logger.LogInformation("Leitura salva no banco: {LeituraId}", leitura.Id);

        // Publica evento no RabbitMQ ANTES de qualquer outra operação
        await PublicarEventoLeituraAsync(sensor, leitura);

        // Verifica alertas
        await VerificarAlertasAsync(sensor, leitura);

        // NÃO atualiza o sensor aqui para evitar problemas de tracking
        // A última leitura será atualizada pelo ProcessamentoDados se necessário

        _logger.LogInformation(
            "Leitura registrada: Sensor={DeviceId}, Valor={Valor}{Unidade}, Qualidade={Qualidade}",
            dto.DeviceId, dto.Valor, dto.Unidade, qualidade);

        return MapToDto(leitura, sensor);
    }

    public async Task<List<LeituraSensorDto>> RegistrarLeituraLoteAsync(RegistrarLeituraLoteDto dto, CancellationToken cancellationToken = default)
    {
        var resultados = new List<LeituraSensorDto>();

        // Agrupa leituras por sensor para otimizar
        var leiturasPorSensor = dto.Leituras.GroupBy(l => l.DeviceId);

        foreach (var grupo in leiturasPorSensor)
        {
            var deviceId = grupo.Key;
            var sensor = await _sensorRepository.ObterPorDeviceIdAsync(deviceId, cancellationToken);

            if (sensor == null)
            {
                _logger.LogWarning("Sensor {DeviceId} não encontrado, leituras ignoradas", deviceId);
                continue;
            }

            if (!sensor.EstaAtivo())
            {
                _logger.LogWarning("Sensor {DeviceId} não está ativo, leituras ignoradas", deviceId);
                continue;
            }

            var leituras = new List<LeituraSensor>();

            foreach (var leituraDto in grupo)
            {
                var qualidade = ValidarLeitura(sensor, leituraDto.Valor);

                var leitura = new LeituraSensor(
                    sensor.Id,
                    leituraDto.Valor,
                    leituraDto.Unidade,
                    leituraDto.TimestampLeitura,
                    qualidade,
                    leituraDto.NivelBateria,
                    leituraDto.IntensidadeSinal,
                    leituraDto.DadosAdicionais
                );

                leituras.Add(leitura);
                resultados.Add(MapToDto(leitura, sensor));
            }

            await _leituraRepository.AdicionarLoteAsync(leituras, cancellationToken);

            // Publica eventos após salvar as leituras
            foreach (var leitura in leituras)
            {
                await PublicarEventoLeituraAsync(sensor, leitura);
            }

            // NÃO atualiza o sensor aqui para evitar problemas de tracking

            _logger.LogInformation(
                "Lote de {Quantidade} leituras registrado para o sensor {DeviceId}",
                leituras.Count, deviceId);
        }

        return resultados;
    }

    public async Task<LeituraSensorDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leitura = await _leituraRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (leitura == null)
            throw new KeyNotFoundException($"Leitura com ID {id} não encontrada");

        return MapToDto(leitura, leitura.Sensor);
    }

    public async Task<List<LeituraSensorDto>> ObterPorSensorAsync(Guid sensorId, int limite = 100, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {sensorId} não encontrado");

        var leituras = await _leituraRepository.ObterPorSensorIdAsync(sensorId, limite, cancellationToken);
        return leituras.Select(l => MapToDto(l, sensor)).ToList();
    }

    public async Task<List<LeituraSensorDto>> ObterPorPropriedadeAsync(Guid propriedadeId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var leituras = await _leituraRepository.ObterPorPropriedadeIdAsync(propriedadeId, dataInicio, dataFim, cancellationToken);
        return leituras.Select(l => MapToDto(l, l.Sensor)).ToList();
    }

    public async Task<List<LeituraSensorDto>> ObterPorPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {sensorId} não encontrado");

        var leituras = await _leituraRepository.ObterPorPeriodoAsync(sensorId, dataInicio, dataFim, cancellationToken);
        return leituras.Select(l => MapToDto(l, sensor)).ToList();
    }

    public async Task<LeituraSensorDto?> ObterUltimaLeituraAsync(Guid sensorId, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {sensorId} não encontrado");

        var leitura = await _leituraRepository.ObterUltimaLeituraAsync(sensorId, cancellationToken);
        
        return leitura != null ? MapToDto(leitura, sensor) : null;
    }

    public async Task<EstatisticasLeituraDto> ObterEstatisticasAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(sensorId, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {sensorId} não encontrado");

        var leituras = await _leituraRepository.ObterPorPeriodoAsync(sensorId, dataInicio, dataFim, cancellationToken);

        var leiturasNormais = leituras.Where(l => l.Qualidade == QualidadeLeitura.Normal).ToList();

        return new EstatisticasLeituraDto(
            sensor.Id,
            sensor.DeviceId,
            sensor.Nome,
            sensor.Tipo,
            dataInicio,
            dataFim,
            leituras.Count,
            leituras.Any() ? leituras.Min(l => l.Valor) : null,
            leituras.Any() ? leituras.Max(l => l.Valor) : null,
            leiturasNormais.Any() ? leiturasNormais.Average(l => l.Valor) : null,
            leituras.Count(l => l.Qualidade == QualidadeLeitura.Normal),
            leituras.Count(l => l.Qualidade == QualidadeLeitura.Suspeita),
            leituras.Count(l => l.Qualidade == QualidadeLeitura.Invalida)
        );
    }

    public async Task MarcarComoSuspeitaAsync(Guid id, string motivo, CancellationToken cancellationToken = default)
    {
        var leitura = await _leituraRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (leitura == null)
            throw new KeyNotFoundException($"Leitura com ID {id} não encontrada");

        leitura.MarcarComoSuspeita(motivo);
        await _leituraRepository.AtualizarAsync(leitura, cancellationToken);

        _logger.LogWarning("Leitura {Id} marcada como suspeita: {Motivo}", id, motivo);
    }

    public async Task MarcarComoInvalidaAsync(Guid id, string motivo, CancellationToken cancellationToken = default)
    {
        var leitura = await _leituraRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (leitura == null)
            throw new KeyNotFoundException($"Leitura com ID {id} não encontrada");

        leitura.MarcarComoInvalida(motivo);
        await _leituraRepository.AtualizarAsync(leitura, cancellationToken);

        _logger.LogWarning("Leitura {Id} marcada como inválida: {Motivo}", id, motivo);
    }

    private QualidadeLeitura ValidarLeitura(Sensor sensor, decimal valor)
    {
        // Validações básicas por tipo de sensor
        switch (sensor.Tipo)
        {
            case TipoSensor.Temperatura:
                if (valor < -50 || valor > 70)
                    return QualidadeLeitura.Suspeita;
                break;
            
            case TipoSensor.UmidadeAr:
            case TipoSensor.UmidadeSolo:
                if (valor < 0 || valor > 100)
                    return QualidadeLeitura.Invalida;
                break;
            
            case TipoSensor.Precipitacao:
                if (valor < 0 || valor > 500) // mm por dia
                    return QualidadeLeitura.Suspeita;
                break;
            
            case TipoSensor.VelocidadeVento:
                if (valor < 0 || valor > 150) // km/h
                    return QualidadeLeitura.Suspeita;
                break;
            
            case TipoSensor.PHSolo:
                if (valor < 0 || valor > 14)
                    return QualidadeLeitura.Invalida;
                break;
        }

        return QualidadeLeitura.Normal;
    }

    private async Task PublicarEventoLeituraAsync(Sensor sensor, LeituraSensor leitura)
    {
        try
        {
            _logger.LogInformation("Iniciando publicação de evento de leitura: {DeviceId} = {Valor}{Unidade}", 
                sensor.DeviceId, leitura.Valor, leitura.Unidade);
            
            var evento = new LeituraRecebidaEvent(
                leitura.Id,
                sensor.Id,
                sensor.DeviceId,
                sensor.PropriedadeId,
                sensor.TalhaoId,
                sensor.Tipo,
                leitura.Valor,
                leitura.Unidade,
                leitura.TimestampLeitura,
                leitura.TimestampRecebimento,
                leitura.Qualidade,
                leitura.NivelBateria,
                leitura.IntensidadeSinal,
                leitura.BateriaBaixa(),
                leitura.SinalFraco(),
                leitura.LatenciaRecebimento(),
                leitura.DadosAdicionais
            );

            _logger.LogInformation("Evento criado, chamando MensageriaService...");
            await _mensageriaService.PublicarLeituraRecebidaAsync(evento);
            _logger.LogInformation("Evento de leitura publicado com sucesso: {DeviceId}", sensor.DeviceId);

            // Atualizar métricas Prometheus
            AtualizarMetricasSensor(sensor, leitura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento de leitura no RabbitMQ: {DeviceId}", sensor.DeviceId);
            // Não falha a operação se o RabbitMQ estiver indisponível
        }
    }



    private async Task VerificarAlertasAsync(Sensor sensor, LeituraSensor leitura)
    {
        try
        {
            // Verifica bateria baixa
            if (leitura.BateriaBaixa())
            {
                var alerta = new AlertaSensorEvent(
                    sensor.Id,
                    sensor.DeviceId,
                    sensor.PropriedadeId,
                    TipoAlerta.BateriaBaixa,
                    $"Bateria do sensor está baixa ({leitura.NivelBateria}%)",
                    DateTime.UtcNow
                );
                await _mensageriaService.PublicarAlertaSensorAsync(alerta);
            }

            // Verifica sinal fraco
            if (leitura.SinalFraco())
            {
                var alerta = new AlertaSensorEvent(
                    sensor.Id,
                    sensor.DeviceId,
                    sensor.PropriedadeId,
                    TipoAlerta.SinalFraco,
                    $"Sinal do sensor está fraco ({leitura.IntensidadeSinal} dBm)",
                    DateTime.UtcNow
                );
                await _mensageriaService.PublicarAlertaSensorAsync(alerta);
            }

            // Verifica valor anômalo
            if (leitura.Qualidade != QualidadeLeitura.Normal)
            {
                var alerta = new AlertaSensorEvent(
                    sensor.Id,
                    sensor.DeviceId,
                    sensor.PropriedadeId,
                    TipoAlerta.ValorAnomalo,
                    $"Leitura com qualidade {leitura.Qualidade}: {leitura.Valor}{leitura.Unidade}",
                    DateTime.UtcNow
                );
                await _mensageriaService.PublicarAlertaSensorAsync(alerta);
            }

            // Verifica se precisa calibração
            if (sensor.PrecisaCalibracao())
            {
                var alerta = new AlertaSensorEvent(
                    sensor.Id,
                    sensor.DeviceId,
                    sensor.PropriedadeId,
                    TipoAlerta.CalibracaoNecessaria,
                    $"Sensor precisa de calibração (última: {sensor.UltimaCalibracao:dd/MM/yyyy})",
                    DateTime.UtcNow
                );
                await _mensageriaService.PublicarAlertaSensorAsync(alerta);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar alertas do sensor");
            // Não falha a operação se houver erro nos alertas
        }
    }

    private static LeituraSensorDto MapToDto(LeituraSensor leitura, Sensor sensor)
    {
        return new LeituraSensorDto(
            leitura.Id,
            leitura.SensorId,
            sensor.DeviceId,
            sensor.Nome,
            sensor.Tipo,
            leitura.Valor,
            leitura.Unidade,
            leitura.TimestampLeitura,
            leitura.TimestampRecebimento,
            leitura.Qualidade,
            leitura.NivelBateria,
            leitura.IntensidadeSinal,
            leitura.BateriaBaixa(),
            leitura.SinalFraco(),
            leitura.LatenciaRecebimento(),
            leitura.DadosAdicionais,
            leitura.Observacoes
        );
    }

    /// <summary>
    /// Atualiza métricas Prometheus com dados do sensor
    /// Nota: Usa ID simplificado do talhão. Nome completo será exibido nos alertas.
    /// </summary>
    private void AtualizarMetricasSensor(Sensor sensor, LeituraSensor leitura)
    {
        try
        {
            var talhaoId = sensor.TalhaoId?.ToString() ?? "unknown";
            // Usar nome mais amigável: apenas primeiros 8 caracteres do GUID
            var talhaoNome = sensor.TalhaoId.HasValue 
                ? sensor.TalhaoId.Value.ToString().Substring(0, 8)
                : "unknown";
            var cultura = "unknown";
            var sensorId = sensor.Id.ToString();

            // Extrair valores específicos por tipo de sensor
            var valorDecimal = (double)leitura.Valor;

            switch (sensor.Tipo)
            {
                case TipoSensor.Temperatura:
                    SensorMetrics.Temperatura
                        .WithLabels(talhaoId, talhaoNome, cultura, sensorId)
                        .Set(valorDecimal);
                    break;

                case TipoSensor.UmidadeSolo:
                    SensorMetrics.Umidade
                        .WithLabels(talhaoId, talhaoNome, cultura, sensorId)
                        .Set(valorDecimal);
                    break;

                case TipoSensor.Precipitacao:
                    SensorMetrics.Precipitacao
                        .WithLabels(talhaoId, talhaoNome, cultura, sensorId)
                        .Set(valorDecimal);
                    break;
            }

            // Incrementar contador de leituras
            if (sensor.TalhaoId.HasValue)
            {
                SensorMetrics.LeiturasRecebidas
                    .WithLabels(talhaoId, talhaoNome)
                    .Inc();
            }

            // Atualizar métricas gerais
            SensorMetrics.LeiturasProcessadas.Inc();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao atualizar métricas do sensor");
            // Não falha a operação se houver erro nas métricas
        }
    }
}


