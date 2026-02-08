using AgroSolutions.IngestaoDados.Application.Events;

namespace AgroSolutions.IngestaoDados.Application.Interfaces;

public interface IMensageriaService
{
    Task PublicarLeituraRecebidaAsync(LeituraRecebidaEvent evento);
    Task PublicarAlertaSensorAsync(AlertaSensorEvent evento);
}
