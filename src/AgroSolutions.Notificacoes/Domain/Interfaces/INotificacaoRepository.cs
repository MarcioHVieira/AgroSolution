using AgroSolutions.Notificacoes.Domain.Entities;
using AgroSolutions.Notificacoes.Domain.Enums;

namespace AgroSolutions.Notificacoes.Domain.Interfaces;

/// <summary>
/// Repositório de Notificações
/// </summary>
public interface INotificacaoRepository
{
    Task<Notificacao?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Notificacao>> ObterTodasAsync();
    Task<IEnumerable<Notificacao>> ObterPorDestinatarioAsync(Guid destinatarioId);
    Task<IEnumerable<Notificacao>> ObterPorStatusAsync(StatusNotificacao status);
    Task<IEnumerable<Notificacao>> ObterPendentesAsync();
    Task<Notificacao> AdicionarAsync(Notificacao notificacao);
    Task AtualizarAsync(Notificacao notificacao);
    Task DeletarAsync(Guid id);
}
