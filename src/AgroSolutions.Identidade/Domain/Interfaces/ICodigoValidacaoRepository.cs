using AgroSolutions.Identidade.Domain.Entities;

namespace AgroSolutions.Identidade.Domain.Interfaces;

/// <summary>
/// Interface de repositório para a entidade CodigoValidacao
/// </summary>
public interface ICodigoValidacaoRepository
{
    Task<CodigoValidacao?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<CodigoValidacao?> ObterUltimoCodigoValidoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(CodigoValidacao codigoValidacao, CancellationToken cancellationToken = default);
    Task AtualizarAsync(CodigoValidacao codigoValidacao, CancellationToken cancellationToken = default);
}
