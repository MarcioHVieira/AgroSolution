namespace AgroSolutions.SharedKernel.Application.Exceptions;

/// <summary>
/// Exceção de validação
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Erros de validação
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base("Um ou mais erros de validação ocorreram")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("Erro de validação")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
}
