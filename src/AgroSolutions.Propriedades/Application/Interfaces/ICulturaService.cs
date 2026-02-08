using AgroSolutions.Propriedades.Application.DTOs;

namespace AgroSolutions.Propriedades.Application.Interfaces;

public interface ICulturaService
{
    Task<CulturaDto> CriarAsync(CriarCulturaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<CulturaDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<CulturaDto>> ObterPorTalhaoAsync(Guid talhaoId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<CulturaDto>> ObterPorPropriedadeAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<CulturaDto>> ObterAtivasAsync(Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<CulturaDto> AtualizarAsync(Guid id, AtualizarCulturaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<CulturaDto> RegistrarColheitaAsync(Guid id, RegistrarColheitaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task CancelarAsync(Guid id, string motivo, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
}
