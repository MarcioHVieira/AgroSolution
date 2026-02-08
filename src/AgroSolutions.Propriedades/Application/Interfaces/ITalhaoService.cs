using AgroSolutions.Propriedades.Application.DTOs;

namespace AgroSolutions.Propriedades.Application.Interfaces;

public interface ITalhaoService
{
    Task<TalhaoDto> CriarAsync(CriarTalhaoDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<TalhaoDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<TalhaoDto>> ObterPorPropriedadeAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<TalhaoDto>> ObterDisponiveisAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<TalhaoDto> AtualizarAsync(Guid id, AtualizarTalhaoDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task MarcarComoEmUsoAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task MarcarComoDisponivelAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task MarcarComoEmDescansoAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
}

