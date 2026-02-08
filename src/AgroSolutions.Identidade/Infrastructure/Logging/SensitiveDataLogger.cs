using System.Text.RegularExpressions;

namespace AgroSolutions.Identidade.Infrastructure.Logging;

/// <summary>
/// Logger que sanitiza dados sensíveis antes de registrar (LGPD)
/// Remove e-mails, CPFs, telefones e outros dados pessoais dos logs
/// </summary>
public partial class SensitiveDataLogger : ILogger
{
    private readonly ILogger _innerLogger;

    // Expressões regulares para detectar dados sensíveis
    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b", RegexOptions.Compiled)]
    private static partial Regex CpfRegex();

    [GeneratedRegex(@"\b\(?\d{2}\)?\s?\d{4,5}-?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex TelefoneRegex();

    [GeneratedRegex(@"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b", RegexOptions.Compiled)]
    private static partial Regex CartaoCreditoRegex();

    public SensitiveDataLogger(ILogger innerLogger)
    {
        _innerLogger = innerLogger ?? throw new ArgumentNullException(nameof(innerLogger));
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var originalMessage = formatter(state, exception);
        var sanitizedMessage = SanitizeMessage(originalMessage);

        _innerLogger.Log(logLevel, eventId, sanitizedMessage, exception, (s, e) => s?.ToString() ?? string.Empty);
    }

    private static string SanitizeMessage(string? message)
    {
        if (string.IsNullOrEmpty(message))
            return message ?? string.Empty;

        // Substituir e-mails por "***@***.***"
        message = EmailRegex().Replace(message, "***@***.***");

        // Substituir CPFs por "***.***.***-**"
        message = CpfRegex().Replace(message, "***.***.***-**");

        // Substituir telefones por "(XX) XXXXX-XXXX"
        message = TelefoneRegex().Replace(message, "(XX) XXXXX-XXXX");

        // Substituir cartões de crédito por "****-****-****-****"
        message = CartaoCreditoRegex().Replace(message, "****-****-****-****");

        return message;
    }


    public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _innerLogger.BeginScope(state);
}
