namespace AgroSolutions.Notificacoes.Application.Interfaces;

public interface IProcessadorNotificacoesService
{
    Task ProcessarNotificacoesPendentesAsync();
}
