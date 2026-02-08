namespace AgroSolutions.Notificacoes.Application.Interfaces;

public interface IEmailService
{
    Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo);
}
