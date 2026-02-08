using AgroSolutions.Propriedades.Domain.Entities;

namespace AgroSolutions.Propriedades.Domain.Interfaces;

public interface IPropriedadeRepository
{
    Task<Propriedade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Propriedade>> ObterPorProprietarioIdAsync(Guid proprietarioId, CancellationToken cancellationToken = default);
    Task<List<Propriedade>> ObterTodasAsync(int pagina = 1, int tamanhoPagina = 20, CancellationToken cancellationToken = default);
    Task<List<Propriedade>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default);
    Task<List<Propriedade>> ObterPorEstadoAsync(string estado, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Propriedade propriedade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Propriedade propriedade, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
