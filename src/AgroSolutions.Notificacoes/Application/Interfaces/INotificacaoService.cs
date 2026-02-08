using AgroSolutions.Notificacoes.Application.DTOs;

namespace AgroSolutions.Notificacoes.Application.Interfaces;

public interface INotificacaoService
{
    Task<NotificacaoDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<NotificacaoDto>> ObterTodasAsync();
    Task<IEnumerable<NotificacaoDto>> ObterPorDestinatarioAsync(Guid destinatarioId);
    Task<NotificacaoDto> CriarAsync(CriarNotificacaoDto dto);
    Task<EstatisticasNotificacoesDto> ObterEstatisticasAsync();
    Task MarcarComoEnviadaAsync(Guid notificacaoId, bool sucesso, string? mensagemErro = null);
}
