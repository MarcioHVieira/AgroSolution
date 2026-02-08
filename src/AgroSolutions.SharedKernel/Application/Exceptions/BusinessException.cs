namespace AgroSolutions.SharedKernel.Application.Exceptions;

/// <summary>
/// Exceção de regra de negócio
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// Código do erro (para identificação)
    /// </summary>
    public string? ErrorCode { get; }

    public BusinessException(string message, string? errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
