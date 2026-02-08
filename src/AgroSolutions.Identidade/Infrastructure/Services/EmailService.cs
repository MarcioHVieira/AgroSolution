using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Configuration.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AgroSolutions.Identidade.Infrastructure.Services;

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

    public async Task EnviarEmailValidacaoAsync(string emailDestino, string nomeUsuario, string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateSettings();

            var assunto = "AgroSolutions - Validação de Conta";
            var corpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .code {{ background-color: #fff; border: 2px dashed #4CAF50; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌾 AgroSolutions</h1>
        </div>
        <div class='content'>
            <h2>Olá, {nomeUsuario}!</h2>
            <p>Bem-vindo à AgroSolutions! Estamos muito felizes em tê-lo conosco.</p>
            <p>Para ativar sua conta, utilize o código de validação abaixo:</p>
            <div class='code'>{codigo}</div>
            <p><strong>Este código é válido por 30 minutos.</strong></p>
            <p>Se você não solicitou este cadastro, ignore este e-mail.</p>
        </div>
        <div class='footer'>
            <p>Atenciosamente,<br>{_emailSettings.FromName}</p>
            <p>Este é um e-mail automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";

            var corpoTexto = $@"
Olá {nomeUsuario},

Bem-vindo à AgroSolutions!

Para ativar sua conta, utilize o código de validação abaixo:

Código: {codigo}

Este código é válido por 30 minutos.

Se você não solicitou este cadastro, ignore este e-mail.

Atenciosamente,
{_emailSettings.FromName}
            ";

            // Cria a mensagem
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(nomeUsuario, emailDestino));
            message.Subject = assunto;

            var builder = new BodyBuilder
            {
                HtmlBody = corpoHtml,
                TextBody = corpoTexto
            };
            message.Body = builder.ToMessageBody();

            // Envia o e-mail
            using var client = await ConfigurarSmtpClientAsync(cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "E-mail de validação enviado com sucesso para {Email}. SMTP: {SmtpServer}:{SmtpPort}",
                emailDestino,
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao enviar e-mail de validação para {Email}. SMTP: {SmtpServer}:{SmtpPort}",
                emailDestino,
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort
            );
            
            // Em desenvolvimento, mostra o código no console como fallback
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                Console.WriteLine($"\n{"=",-60}");
                Console.WriteLine($"FALLBACK - E-MAIL DE VALIDAÇÃO (Erro no envio SMTP)");
                Console.WriteLine($"{"=",-60}");
                Console.WriteLine($"Para: {emailDestino}");
                Console.WriteLine($"Código de Validação: {codigo}");
                Console.WriteLine($"{"=",-60}\n");
            }
            
            throw new InvalidOperationException($"Falha ao enviar e-mail para {emailDestino}. Verifique as configurações de SMTP.", ex);
        }
    }

    public async Task EnviarEmailRecuperacaoSenhaAsync(string emailDestino, string nomeUsuario, string codigo, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateSettings();

            var assunto = "AgroSolutions - Recuperação de Senha";
            var corpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #FF5722; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .code {{ background-color: #fff; border: 2px dashed #FF5722; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔒 AgroSolutions</h1>
        </div>
        <div class='content'>
            <h2>Olá {nomeUsuario}!</h2>
            <p>Você solicitou a recuperação de sua senha.</p>
            <p>Para redefinir sua senha, utilize o código abaixo:</p>
            <div class='code'>{codigo}</div>
            <p><strong>Este código é válido por 30 minutos.</strong></p>
            <div class='warning'>
                <strong>⚠️ Importante:</strong> Se você não solicitou esta recuperação, ignore este e-mail e sua senha permanecerá inalterada.
            </div>
        </div>
        <div class='footer'>
            <p>Atenciosamente,<br>{_emailSettings.FromName}</p>
            <p>Este é um e-mail automático, por favor não responda.</p>
        </div>
    </div>
</body>
</html>";

            var corpoTexto = $@"
Olá {nomeUsuario},

Você solicitou a recuperação de sua senha.

Para redefinir sua senha, utilize o código abaixo:

Código: {codigo}

Este código é válido por 30 minutos.

⚠️ IMPORTANTE: Se você não solicitou esta recuperação, ignore este e-mail e sua senha permanecerá inalterada.

Atenciosamente,
{_emailSettings.FromName}
            ";

            // Cria a mensagem
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(nomeUsuario, emailDestino));
            message.Subject = assunto;

            var builder = new BodyBuilder
            {
                HtmlBody = corpoHtml,
                TextBody = corpoTexto
            };
            message.Body = builder.ToMessageBody();

            // Envia o e-mail
            using var client = await ConfigurarSmtpClientAsync(cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "E-mail de recuperação de senha enviado com sucesso para {Email}",
                emailDestino
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao enviar e-mail de recuperação de senha para {Email}",
                emailDestino
            );
            
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                Console.WriteLine($"\n{"=",-60}");
                Console.WriteLine($"FALLBACK - E-MAIL DE RECUPERAÇÃO (Erro no envio SMTP)");
                Console.WriteLine($"{"=",-60}");
                Console.WriteLine($"Para: {emailDestino}");
                Console.WriteLine($"Código de Recuperação: {codigo}");
                Console.WriteLine($"{"=",-60}\n");
            }
            
            throw new InvalidOperationException($"Falha ao enviar e-mail para {emailDestino}. Verifique as configurações de SMTP.", ex);
        }
    }

    public async Task EnviarEmailExclusaoContaAsync(string emailDestino, string nomeUsuario, DateTime dataExclusaoFinal, CancellationToken cancellationToken = default)
    {
        try
        {
            var assunto = "Confirmação de Exclusão de Conta - AgroSolutions";

            var corpoHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #dc3545; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .alert {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .danger {{ background-color: #f8d7da; border-left: 4px solid #dc3545; padding: 15px; margin: 20px 0; }}
        .info {{ background-color: #d1ecf1; border-left: 4px solid #0c5460; padding: 15px; margin: 20px 0; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🛡️ AgroSolutions</h1>
        </div>
        <div class='content'>
            <h2>Olá {nomeUsuario},</h2>
            <div class='danger'>
                <h3>⚠️ Solicitação de Exclusão de Conta Recebida</h3>
                <p>Confirmamos o recebimento de sua solicitação de exclusão de conta conforme seu direito garantido pela LGPD (Lei Geral de Proteção de Dados - Art. 18, VI).</p>
            </div>
            
            <h3>📅 O que acontecerá agora?</h3>
            <ol>
                <li><strong>Período de Carência:</strong> Sua conta será desativada imediatamente.</li>
                <li><strong>Exclusão Final:</strong> Todos os seus dados serão permanentemente removidos em <strong>{dataExclusaoFinal:dd/MM/yyyy}</strong>.</li>
                <li><strong>Reversão:</strong> Durante este período, você pode cancelar a exclusão entrando em contato conosco.</li>
            </ol>

            <div class='info'>
                <h4>📋 Dados que serão excluídos:</h4>
                <ul>
                    <li>Informações pessoais (nome, e-mail, telefone, CPF)</li>
                    <li>Histórico de propriedades e talhões cadastrados</li>
                    <li>Dados de sensores e leituras</li>
                    <li>Alertas e notificações</li>
                    <li>Logs de acesso e atividades</li>
                </ul>
            </div>

            <div class='alert'>
                <strong>💡 Importante:</strong> Se você não solicitou esta exclusão, entre em contato conosco imediatamente em <a href='mailto:suporte@agrosolutions.com.br'>suporte@agrosolutions.com.br</a>.
            </div>

            <p>Sentiremos sua falta! 😢</p>
            <p>Se você mudou de ideia, estamos aqui para ajudar.</p>
        </div>
        <div class='footer'>
            <p>Atenciosamente,<br>{_emailSettings.FromName}</p>
            <p>📧 suporte@agrosolutions.com.br | 📞 (11) 1234-5678</p>
            <p><small>Conforme LGPD Lei nº 13.709/2018 - Art. 18, VI (Direito ao Esquecimento)</small></p>
        </div>
    </div>
</body>
</html>";

            var corpoTexto = $@"
Olá {nomeUsuario},

SOLICITAÇÃO DE EXCLUSÃO DE CONTA RECEBIDA

Confirmamos o recebimento de sua solicitação de exclusão de conta conforme seu direito garantido pela LGPD (Lei Geral de Proteção de Dados - Art. 18, VI).

O QUE ACONTECERÁ AGORA?

1. Período de Carência: Sua conta será desativada imediatamente.
2. Exclusão Final: Todos os seus dados serão permanentemente removidos em {dataExclusaoFinal:dd/MM/yyyy}.
3. Reversão: Durante este período, você pode cancelar a exclusão entrando em contato conosco.

DADOS QUE SERÃO EXCLUÍDOS:
- Informações pessoais (nome, e-mail, telefone, CPF)
- Histórico de propriedades e talhões cadastrados
- Dados de sensores e leituras
- Alertas e notificações
- Logs de acesso e atividades

IMPORTANTE: Se você não solicitou esta exclusão, entre em contato conosco imediatamente em suporte@agrosolutions.com.br.

Sentiremos sua falta!
Se você mudou de ideia, estamos aqui para ajudar.

Atenciosamente,
{_emailSettings.FromName}

suporte@agrosolutions.com.br | (11) 1234-5678
Conforme LGPD Lei nº 13.709/2018 - Art. 18, VI (Direito ao Esquecimento)
            ";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(nomeUsuario, emailDestino));
            message.Subject = assunto;

            var builder = new BodyBuilder
            {
                HtmlBody = corpoHtml,
                TextBody = corpoTexto
            };
            message.Body = builder.ToMessageBody();

            using var client = await ConfigurarSmtpClientAsync(cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-mail de confirmação de exclusão enviado para {Email}", emailDestino);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail de exclusão de conta para {Email}", emailDestino);
            throw;
        }
    }

    public async Task EnviarEmailGenericoAsync(string emailDestino, string assunto, string corpoHtml, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateSettings();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
            message.To.Add(new MailboxAddress(emailDestino, emailDestino));
            message.Subject = assunto;

            var bodyBuilder = new BodyBuilder { HtmlBody = corpoHtml };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = await ConfigurarSmtpClientAsync(cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("E-mail genérico enviado com sucesso para {Email}. Assunto: {Assunto}", emailDestino, assunto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail genérico para {Email}. Assunto: {Assunto}", emailDestino, assunto);
            throw;
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
}





