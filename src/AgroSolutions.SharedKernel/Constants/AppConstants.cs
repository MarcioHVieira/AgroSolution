namespace AgroSolutions.SharedKernel.Constants;

/// <summary>
/// Constantes globais da aplicação
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Tamanho padrão de página para paginação
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Tamanho máximo de página
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Formato padrão de data
    /// </summary>
    public const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Formato padrão de data/hora
    /// </summary>
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Cultura padrão (pt-BR)
    /// </summary>
    public const string DefaultCulture = "pt-BR";

    /// <summary>
    /// Timeout padrão para requisições HTTP (segundos)
    /// </summary>
    public const int DefaultHttpTimeoutSeconds = 30;

    /// <summary>
    /// Máximo de tentativas de retry
    /// </summary>
    public const int MaxRetryAttempts = 3;
}
