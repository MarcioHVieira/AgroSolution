using AgroSolutions.Propriedades.Domain.Entities;

namespace AgroSolutions.Propriedades.Domain.Interfaces;

public interface ITalhaoRepository
{
    Task<Talhao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Talhao>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<List<Talhao>> ObterDisponiveisPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Talhao talhao, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Talhao talhao, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
