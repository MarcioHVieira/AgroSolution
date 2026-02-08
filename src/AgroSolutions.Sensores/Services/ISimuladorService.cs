using AgroSolutions.Sensores.Models;

namespace AgroSolutions.Sensores.Services;

public interface ISimuladorService
{
    Task<ResultadoSimulacaoDto> SimularSecaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true);
    Task<ResultadoSimulacaoDto> SimularGeadaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true);
    Task<ResultadoSimulacaoDto> SimularCalorExcessivoAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true);
    Task<ResultadoSimulacaoDto> SimularExcessoUmidadeAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true);
    Task<ResultadoSimulacaoDto> SimularRiscoPragaAsync(Guid talhaoId, Severidade severidade, bool enviarParaApi = true);
    Task<ResultadoSimulacaoDto> SimularCenarioCompletoAsync(Guid talhaoId);
}
