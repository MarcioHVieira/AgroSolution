namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface para envio de e-mails
/// </summary>
public interface IEmailService
{
    Task EnviarEmailValidacaoAsync(string emailDestino, string nomeUsuario, string codigo, CancellationToken cancellationToken = default);
    Task EnviarEmailRecuperacaoSenhaAsync(string emailDestino, string nomeUsuario, string codigo, CancellationToken cancellationToken = default);
    Task EnviarEmailExclusaoContaAsync(string emailDestino, string nomeUsuario, DateTime dataExclusaoFinal, CancellationToken cancellationToken = default);
    Task EnviarEmailGenericoAsync(string emailDestino, string assunto, string corpoHtml, CancellationToken cancellationToken = default);
}
