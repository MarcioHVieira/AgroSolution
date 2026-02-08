using AgroSolutions.ProcessamentoDados.Application.DTOs;

namespace AgroSolutions.ProcessamentoDados.Application.Interfaces;

/// <summary>
/// Serviço de agregação de dados
/// </summary>
public interface IAgregacaoService
{
    Task GerarAgregacaoHorariaAsync(Guid sensorId, DateTime hora);
    Task GerarAgregacaoDiariaAsync(Guid sensorId, DateTime dia);
    Task GerarAgregacaoSemanalAsync(Guid sensorId, DateTime semana);
    Task GerarAgregacaoMensalAsync(Guid sensorId, DateTime mes);
    Task<AgregacaoDadosDto?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<AgregacaoDadosDto>> ConsultarAgregacoesAsync(ConsultarAgregacoesDto filtros);
    Task<bool> AgregacaoExisteAsync(Guid sensorId, string tipoAgregacao, DateTime periodo);
}
