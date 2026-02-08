using AgroSolutions.IngestaoDados.Application.DTOs;
using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Application.Interfaces;

public interface ISensorService
{
    Task<SensorDto> CriarAsync(CriarSensorDto dto, CancellationToken cancellationToken = default);
    Task<SensorDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SensorDto> ObterPorDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<List<SensorDto>> ObterPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<List<SensorDto>> ObterPorTalhaoAsync(Guid talhaoId, CancellationToken cancellationToken = default);
    Task<List<SensorDto>> ObterPorTipoAsync(TipoSensor tipo, CancellationToken cancellationToken = default);
    Task<List<SensorDto>> ObterAtivosPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<SensorDto> AtualizarAsync(Guid id, AtualizarSensorDto dto, CancellationToken cancellationToken = default);
    Task AtualizarStatusAsync(Guid id, StatusSensor status, CancellationToken cancellationToken = default);
    Task RegistrarCalibracaoAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
