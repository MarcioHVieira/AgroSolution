namespace AgroSolutions.Analise.Application.Interfaces;

/// <summary>
/// Interface do serviço de Regras de Alerta
/// </summary>
public interface IRegraAlertaService
{
    Task<DTOs.RegraAlertaDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<DTOs.RegraAlertaDto>> ObterTodasAsync();
    Task<IEnumerable<DTOs.RegraAlertaDto>> ObterAtivasAsync();
    Task<DTOs.RegraAlertaDto> CriarAsync(DTOs.CriarRegraAlertaDto dto);
    Task AtualizarAsync(Guid id, DTOs.CriarRegraAlertaDto dto);
    Task DeletarAsync(Guid id);
    Task AtivarDesativarAsync(Guid id, bool ativa);
}
