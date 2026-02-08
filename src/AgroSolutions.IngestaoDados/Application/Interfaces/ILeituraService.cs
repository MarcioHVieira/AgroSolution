using AgroSolutions.IngestaoDados.Application.DTOs;

namespace AgroSolutions.IngestaoDados.Application.Interfaces;

public interface ILeituraService
{
    Task<LeituraSensorDto> RegistrarLeituraAsync(RegistrarLeituraDto dto, CancellationToken cancellationToken = default);
    Task<List<LeituraSensorDto>> RegistrarLeituraLoteAsync(RegistrarLeituraLoteDto dto, CancellationToken cancellationToken = default);
    Task<LeituraSensorDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<LeituraSensorDto>> ObterPorSensorAsync(Guid sensorId, int limite = 100, CancellationToken cancellationToken = default);
    Task<List<LeituraSensorDto>> ObterPorPropriedadeAsync(Guid propriedadeId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<List<LeituraSensorDto>> ObterPorPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<LeituraSensorDto?> ObterUltimaLeituraAsync(Guid sensorId, CancellationToken cancellationToken = default);
    Task<EstatisticasLeituraDto> ObterEstatisticasAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task MarcarComoSuspeitaAsync(Guid id, string motivo, CancellationToken cancellationToken = default);
    Task MarcarComoInvalidaAsync(Guid id, string motivo, CancellationToken cancellationToken = default);
}
