using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Domain.Interfaces;

public interface ISensorRepository
{
    Task<Sensor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sensor?> ObterPorDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterPorTalhaoIdAsync(Guid talhaoId, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterPorTipoAsync(TipoSensor tipo, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterPorStatusAsync(StatusSensor status, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterAtivosPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<List<Sensor>> ObterSensoresComBateriaBaixaAsync(CancellationToken cancellationToken = default);
    Task<bool> DeviceIdExisteAsync(string deviceId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Sensor sensor, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
