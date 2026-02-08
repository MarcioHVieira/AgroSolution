namespace AgroSolutions.Analise.Application.Interfaces;

/// <summary>
/// Interface do serviço de Alertas
/// </summary>
public interface IAlertaService
{
    Task<DTOs.AlertaDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<DTOs.AlertaDto>> ObterTodosPorTalhaoAsync(Guid talhaoId);
    Task<IEnumerable<DTOs.AlertaDto>> ObterAtivosAsync();
    Task<DTOs.AlertaDto> CriarAsync(DTOs.CriarAlertaDto dto);
    Task AtualizarStatusAsync(Guid id, DTOs.AtualizarStatusAlertaDto dto);
    Task MarcarComoVisualizadoAsync(Guid id);
    Task MarcarComoResolvidoAsync(Guid id);
    Task<DTOs.EstatisticasAlertasDto> ObterEstatisticasAsync();
}
