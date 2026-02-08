using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Application.Interfaces;
using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;

namespace AgroSolutions.IngestaoDados.Application.Services;

public class SensorService : ISensorService
{
    private readonly ISensorRepository _sensorRepository;
    private readonly ILogger<SensorService> _logger;

    public SensorService(
        ISensorRepository sensorRepository,
        ILogger<SensorService> logger)
    {
        _sensorRepository = sensorRepository;
        _logger = logger;
    }

    public async Task<SensorDto> CriarAsync(CriarSensorDto dto, CancellationToken cancellationToken = default)
    {
        // Verifica se DeviceId já existe
        if (await _sensorRepository.DeviceIdExisteAsync(dto.DeviceId, cancellationToken))
            throw new InvalidOperationException($"DeviceId '{dto.DeviceId}' já está em uso");

        var sensor = new Sensor(
            dto.PropriedadeId,
            dto.DeviceId,
            dto.Nome,
            dto.Tipo,
            dto.IntervaloLeituraMinutos,
            dto.TalhaoId,
            dto.Fabricante,
            dto.Modelo,
            dto.Latitude,
            dto.Longitude,
            dto.Altitude,
            dto.Observacoes
        );

        await _sensorRepository.AdicionarAsync(sensor, cancellationToken);

        _logger.LogInformation(
            "Sensor criado: DeviceId={DeviceId}, Tipo={Tipo}, Propriedade={PropriedadeId}",
            sensor.DeviceId, sensor.Tipo, sensor.PropriedadeId);

        return MapToDto(sensor);
    }

    public async Task<SensorDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {id} não encontrado");

        return MapToDto(sensor);
    }

    public async Task<SensorDto> ObterPorDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorDeviceIdAsync(deviceId, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com DeviceId '{deviceId}' não encontrado");

        return MapToDto(sensor);
    }

    public async Task<List<SensorDto>> ObterPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        var sensores = await _sensorRepository.ObterPorPropriedadeIdAsync(propriedadeId, cancellationToken);
        return sensores.Select(MapToDto).ToList();
    }

    public async Task<List<SensorDto>> ObterPorTalhaoAsync(Guid talhaoId, CancellationToken cancellationToken = default)
    {
        var sensores = await _sensorRepository.ObterPorTalhaoIdAsync(talhaoId, cancellationToken);
        return sensores.Select(MapToDto).ToList();
    }

    public async Task<List<SensorDto>> ObterPorTipoAsync(TipoSensor tipo, CancellationToken cancellationToken = default)
    {
        var sensores = await _sensorRepository.ObterPorTipoAsync(tipo, cancellationToken);
        return sensores.Select(MapToDto).ToList();
    }

    public async Task<List<SensorDto>> ObterAtivosPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        var sensores = await _sensorRepository.ObterAtivosPorPropriedadeAsync(propriedadeId, cancellationToken);
        return sensores.Select(MapToDto).ToList();
    }

    public async Task<SensorDto> AtualizarAsync(Guid id, AtualizarSensorDto dto, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {id} não encontrado");

        sensor.Atualizar(
            dto.Nome,
            dto.IntervaloLeituraMinutos,
            dto.TalhaoId,
            dto.Fabricante,
            dto.Modelo,
            dto.Latitude,
            dto.Longitude,
            dto.Altitude,
            dto.Observacoes
        );

        await _sensorRepository.AtualizarAsync(sensor, cancellationToken);

        _logger.LogInformation("Sensor {Id} atualizado com sucesso", id);

        return MapToDto(sensor);
    }

    public async Task AtualizarStatusAsync(Guid id, StatusSensor status, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {id} não encontrado");

        sensor.AtualizarStatus(status);
        await _sensorRepository.AtualizarAsync(sensor, cancellationToken);

        _logger.LogInformation("Status do sensor {Id} alterado para {Status}", id, status);
    }

    public async Task RegistrarCalibracaoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {id} não encontrado");

        sensor.RegistrarCalibracao();
        await _sensorRepository.AtualizarAsync(sensor, cancellationToken);

        _logger.LogInformation("Calibração registrada para o sensor {Id}", id);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sensor = await _sensorRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (sensor == null)
            throw new KeyNotFoundException($"Sensor com ID {id} não encontrado");

        await _sensorRepository.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("Sensor {Id} removido com sucesso", id);
    }

    private static SensorDto MapToDto(Sensor sensor)
    {
        return new SensorDto(
            sensor.Id,
            sensor.PropriedadeId,
            sensor.TalhaoId,
            sensor.DeviceId,
            sensor.Nome,
            sensor.Tipo,
            sensor.Fabricante,
            sensor.Modelo,
            sensor.Latitude,
            sensor.Longitude,
            sensor.Altitude,
            sensor.IntervaloLeituraMinutos,
            sensor.Status,
            sensor.UltimaLeitura,
            sensor.UltimaCalibracao,
            sensor.PrecisaCalibracao(),
            sensor.Observacoes,
            sensor.DataCadastro,
            sensor.DataAtualizacao
        );
    }
}

