using AgroSolutions.Identidade.Domain.Entities;

namespace AgroSolutions.Identidade.Domain.Interfaces;

/// <summary>
/// Interface de repositório para a entidade Usuario
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<IEnumerable<Usuario>> ListarTodosAsync(CancellationToken cancellationToken = default);
    Task<List<Usuario>> ObterMarcadosParaExclusaoAsync(DateTime dataLimite, CancellationToken cancellationToken = default);
}
