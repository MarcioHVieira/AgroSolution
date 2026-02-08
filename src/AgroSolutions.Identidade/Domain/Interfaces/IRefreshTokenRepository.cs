using AgroSolutions.Identidade.Domain.Entities;

namespace AgroSolutions.Identidade.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<RefreshToken?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> ObterTodosPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task AtualizarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevogarTodosDoUsuarioAsync(Guid usuarioId, string motivo, CancellationToken cancellationToken = default);
    Task RemoverExpiradosAsync(CancellationToken cancellationToken = default);
}
