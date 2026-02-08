using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;

namespace AgroSolutions.Analise.Domain.Interfaces;

/// <summary>
/// Repositório de Alertas
/// </summary>
public interface IAlertaRepository
{
    Task<Alerta?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Alerta>> ObterTodosPorTalhaoAsync(Guid talhaoId);
    Task<IEnumerable<Alerta>> ObterAtivosAsync();
    Task<IEnumerable<Alerta>> ObterPorStatusAsync(StatusAlerta status);
    Task<IEnumerable<Alerta>> ObterPorTipoAsync(TipoAlerta tipo);
    Task<Alerta> AdicionarAsync(Alerta alerta);
    Task AtualizarAsync(Alerta alerta);
    Task DeletarAsync(Guid id);
    Task<bool> ExisteAlertaAtivoAsync(Guid talhaoId, TipoAlerta tipo);
}
