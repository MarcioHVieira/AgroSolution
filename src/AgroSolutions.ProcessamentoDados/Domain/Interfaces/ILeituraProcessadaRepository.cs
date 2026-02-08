using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Domain.Interfaces;

public interface ILeituraProcessadaRepository
{
    Task<LeituraProcessada?> ObterPorIdAsync(Guid id);
    Task<LeituraProcessada?> ObterPorLeituraOrigemIdAsync(Guid leituraOrigemId);
    Task<IEnumerable<LeituraProcessada>> ObterPorSensorAsync(Guid sensorId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<LeituraProcessada>> ObterPorPropriedadeAsync(Guid propriedadeId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<LeituraProcessada>> ObterPorTalhaoAsync(Guid talhaoId, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<LeituraProcessada>> ObterComFalhaAsync(int limit = 100);
    Task AdicionarAsync(LeituraProcessada leitura);
    Task AtualizarAsync(LeituraProcessada leitura);
    Task<int> ContarPorStatusAsync(StatusProcessamento status);
}
