using AgroSolutions.Propriedades.Application.DTOs;

namespace AgroSolutions.Propriedades.Application.Interfaces;

public interface IPropriedadeService
{
    Task<PropriedadeDto> CriarAsync(Guid proprietarioId, CriarPropriedadeDto dto, CancellationToken cancellationToken = default);
    Task<PropriedadeDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<List<PropriedadeDto>> ObterPorProprietarioAsync(Guid proprietarioId, CancellationToken cancellationToken = default);
    Task<List<PropriedadeDto>> ObterTodasAsync(Guid usuarioId, bool ehAdmin, int pagina = 1, int tamanhoPagina = 20, CancellationToken cancellationToken = default);
    Task<PropriedadeDto> AtualizarAsync(Guid id, AtualizarPropriedadeDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task<PropriedadeDto> AtualizarEnderecoAsync(Guid id, AtualizarEnderecoPropriedadeDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task AtivarAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task InativarAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default);
}
