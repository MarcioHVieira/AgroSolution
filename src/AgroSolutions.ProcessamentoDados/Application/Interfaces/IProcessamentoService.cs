using AgroSolutions.ProcessamentoDados.Application.DTOs;
using AgroSolutions.ProcessamentoDados.Application.Events;

namespace AgroSolutions.ProcessamentoDados.Application.Interfaces;

/// <summary>
/// Serviço de processamento de leituras de sensores
/// </summary>
public interface IProcessamentoService
{
    Task ProcessarLeituraAsync(LeituraRecebidaEvent evento);
    Task<LeituraProcessadaDto?> ObterPorIdAsync(Guid id);
    Task<LeituraProcessadaDto?> ObterPorLeituraOrigemIdAsync(Guid leituraOrigemId);
    Task<IEnumerable<LeituraProcessadaDto>> ConsultarLeiturasAsync(ConsultarLeiturasDto filtros);
    Task<EstatisticasProcessamentoDto> ObterEstatisticasAsync(DateTime dataInicio, DateTime dataFim);
    Task ReprocessarFalhasAsync(int limite = 100);
    Task<int> ContarPorStatusAsync(string status);
}
