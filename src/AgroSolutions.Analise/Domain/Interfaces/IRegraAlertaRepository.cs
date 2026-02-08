using AgroSolutions.Analise.Domain.Entities;

namespace AgroSolutions.Analise.Domain.Interfaces;

/// <summary>
/// Repositório de Regras de Alerta
/// </summary>
public interface IRegraAlertaRepository
{
    Task<RegraAlerta?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<RegraAlerta>> ObterTodasAsync();
    Task<IEnumerable<RegraAlerta>> ObterAtivasAsync();
    Task<RegraAlerta> AdicionarAsync(RegraAlerta regra);
    Task AtualizarAsync(RegraAlerta regra);
    Task DeletarAsync(Guid id);
}
