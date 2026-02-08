using AgroSolutions.IngestaoDados.Domain.Entities;

namespace AgroSolutions.IngestaoDados.Domain.Interfaces;

public interface ILeituraSensorRepository
{
    Task<LeituraSensor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LeituraSensor>> ObterPorSensorIdAsync(Guid sensorId, int limite = 100, CancellationToken cancellationToken = default);
    Task<List<LeituraSensor>> ObterPorPropriedadeIdAsync(Guid propriedadeId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<List<LeituraSensor>> ObterPorPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<LeituraSensor?> ObterUltimaLeituraAsync(Guid sensorId, CancellationToken cancellationToken = default);
    Task<List<LeituraSensor>> ObterLeiturasAnomalasAsync(Guid sensorId, CancellationToken cancellationToken = default);
    Task<decimal?> ObterMediaPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task AdicionarAsync(LeituraSensor leitura, CancellationToken cancellationToken = default);
    Task AdicionarLoteAsync(List<LeituraSensor> leituras, CancellationToken cancellationToken = default);
    Task AtualizarAsync(LeituraSensor leitura, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
    Task RemoverAntigasAsync(DateTime dataLimite, CancellationToken cancellationToken = default);
}
