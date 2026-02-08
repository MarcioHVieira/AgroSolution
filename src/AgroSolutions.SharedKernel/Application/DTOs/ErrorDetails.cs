namespace AgroSolutions.SharedKernel.Application.DTOs;

/// <summary>
/// Detalhes de erro para respostas de API
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Código HTTP do erro
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Código do erro (para identificação)
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Tipo do erro (nome da exception)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Mensagem de erro
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detalhes adicionais
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Stack trace (apenas em desenvolvimento)
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// ID de rastreamento
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp do erro
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Cria ErrorDetails a partir de uma Exception
    /// </summary>
    public static ErrorDetails FromException(Exception exception)
    {
        return new ErrorDetails
        {
            Type = exception.GetType().Name,
            Message = exception.Message,
            Details = exception.InnerException?.Message,
            StackTrace = exception.StackTrace
        };
    }

    /// <summary>
    /// Construtor com 4 parâmetros (para compatibilidade)
    /// </summary>
    public ErrorDetails()
    {
    }

    /// <summary>
    /// Construtor com parâmetros
    /// </summary>
    public ErrorDetails(int statusCode, string type, string message, string traceId)
    {
        StatusCode = statusCode;
        Type = type;
        Message = message;
        TraceId = traceId;
    }

}

