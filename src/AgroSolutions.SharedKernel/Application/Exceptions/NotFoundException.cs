namespace AgroSolutions.SharedKernel.Application.Exceptions;

/// <summary>
/// Exceção quando entidade não é encontrada
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entity, object id) 
        : base($"{entity} com Id '{id}' não encontrado")
    {
    }

    public NotFoundException(string message) 
        : base(message)
    {
    }
}
