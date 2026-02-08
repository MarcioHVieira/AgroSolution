using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Configuration.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgroSolutions.Notificacoes.Application.Services;

/// <summary>
/// Serviço de envio de e-mails usando SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> EnviarEmailAsync(string destinatario, string assunto, string corpo)
    {
        try
        {
            ValidateSettings();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(MailboxAddress.Parse(destinatario));
            message.Subject = assunto;

            var builder = new BodyBuilder
            {
                HtmlBody = corpo,
                TextBody = ExtractTextFromHtml(corpo) // Cria versão texto automática
            };
            message.Body = builder.ToMessageBody();

            using var client = await ConfigurarSmtpClientAsync(CancellationToken.None);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation(
                "E-mail enviado com sucesso para {Destinatario}. SMTP: {SmtpServer}:{SmtpPort}",
                destinatario,
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort
            );
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao enviar e-mail para {Destinatario}. SMTP: {SmtpServer}:{SmtpPort}",
                destinatario,
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort
            );

            // Fallback em modo debug
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                Console.WriteLine($"\n{"=",-60}");
                Console.WriteLine($"FALLBACK - E-MAIL DE ALERTA (Erro no envio SMTP)");
                Console.WriteLine($"{"=",-60}");
                Console.WriteLine($"Para: {destinatario}");
                Console.WriteLine($"Assunto: {assunto}");
                Console.WriteLine($"{"=",-60}\n");
            }

            return false;
        }
    }

    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer))
            throw new InvalidOperationException("Email SmtpServer não configurado.");

        if (_emailSettings.SmtpPort <= 0)
            throw new InvalidOperationException("Email SmtpPort inválido.");

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpUser))
            throw new InvalidOperationException("Email SmtpUser não configurado.");

        if (string.IsNullOrWhiteSpace(_emailSettings.SmtpPassword))
            throw new InvalidOperationException("Email SmtpPassword não configurado.");

        if (string.IsNullOrWhiteSpace(_emailSettings.FromEmail))
            throw new InvalidOperationException("Email FromEmail não configurado.");
    }

    private async Task<SmtpClient> ConfigurarSmtpClientAsync(CancellationToken cancellationToken)
    {
        var client = new SmtpClient
        {
            Timeout = _emailSettings.TimeoutSeconds * 1000 // Converte segundos para milissegundos
        };

        var secureSocketOptions = _emailSettings.EnableSsl 
            ? SecureSocketOptions.StartTls 
            : SecureSocketOptions.None;

        await client.ConnectAsync(
            _emailSettings.SmtpServer, 
            _emailSettings.SmtpPort, 
            secureSocketOptions, 
            cancellationToken);
        
        await client.AuthenticateAsync(
            _emailSettings.SmtpUser, 
            _emailSettings.SmtpPassword, 
            cancellationToken);

        return client;
    }

    /// <summary>
    /// Extrai texto simples do HTML para versão texto do email
    /// </summary>
    private string ExtractTextFromHtml(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        // Remove tags HTML básicas
        var text = System.Text.RegularExpressions.Regex.Replace(html, @"<[^>]+>", string.Empty);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}

