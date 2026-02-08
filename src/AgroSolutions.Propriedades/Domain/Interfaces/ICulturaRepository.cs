using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;

namespace AgroSolutions.Propriedades.Domain.Interfaces;

public interface ICulturaRepository
{
    Task<Cultura?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Cultura>> ObterPorTalhaoIdAsync(Guid talhaoId, CancellationToken cancellationToken = default);
    Task<List<Cultura>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<List<Cultura>> ObterAtivasAsync(CancellationToken cancellationToken = default);
    Task<List<Cultura>> ObterPorTipoAsync(TipoCultura tipo, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Cultura cultura, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Cultura cultura, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
