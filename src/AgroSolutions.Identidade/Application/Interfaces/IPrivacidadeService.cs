using AgroSolutions.Identidade.Application.DTOs;

namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface do serviço de privacidade (LGPD)
/// </summary>
public interface IPrivacidadeService
{
    Task<DadosExportadosDto> ExportarDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task SolicitarExclusaoContaAsync(Guid usuarioId, string motivo, CancellationToken cancellationToken = default);
    Task<List<ConsentimentoDto>> ObterHistoricoConsentimentosAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
