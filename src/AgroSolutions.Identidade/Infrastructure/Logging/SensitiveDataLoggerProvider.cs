namespace AgroSolutions.Identidade.Infrastructure.Logging;

/// <summary>
/// Provider de logger que sanitiza dados sensíveis
/// Wrapper para adicionar sanitização a qualquer ILoggerProvider
/// </summary>
public class SensitiveDataLoggerProvider : ILoggerProvider
{
    private readonly ILoggerProvider _innerProvider;

    public SensitiveDataLoggerProvider(ILoggerProvider innerProvider)
    {
        _innerProvider = innerProvider ?? throw new ArgumentNullException(nameof(innerProvider));
    }

    public ILogger CreateLogger(string categoryName)
    {
        var innerLogger = _innerProvider.CreateLogger(categoryName);
        return new SensitiveDataLogger(innerLogger);
    }

    public void Dispose()
    {
        _innerProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
