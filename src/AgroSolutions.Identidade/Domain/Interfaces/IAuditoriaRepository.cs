using AgroSolutions.Identidade.Domain.Entities;

namespace AgroSolutions.Identidade.Domain.Interfaces;

/// <summary>
/// Interface do repositório de auditoria
/// </summary>
public interface IAuditoriaRepository
{
    Task AdicionarAsync(AuditoriaAcesso auditoria, CancellationToken cancellationToken = default);
    Task<List<AuditoriaAcesso>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<List<AuditoriaAcesso>> ObterPorPeriodoAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
    Task<List<AuditoriaAcesso>> ObterPorAcaoAsync(string acao, CancellationToken cancellationToken = default);
    Task<int> ContarTentativasLoginFalhasAsync(string email, DateTime desde, CancellationToken cancellationToken = default);
}
