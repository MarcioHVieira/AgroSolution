using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Domain.Interfaces;

public interface IAgregacaoDadosRepository
{
    Task<AgregacaoDados?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<AgregacaoDados>> ObterPorSensorAsync(Guid sensorId, TipoAgregacao tipo, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<IEnumerable<AgregacaoDados>> ObterPorPropriedadeAsync(Guid propriedadeId, TipoAgregacao tipo, DateTime? dataInicio = null, DateTime? dataFim = null);
    Task<AgregacaoDados?> ObterPorPeriodoAsync(Guid sensorId, TipoAgregacao tipo, DateTime periodoInicio);
    Task AdicionarAsync(AgregacaoDados agregacao);
    Task AtualizarAsync(AgregacaoDados agregacao);
}
