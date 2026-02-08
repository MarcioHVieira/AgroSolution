namespace AgroSolutions.SharedKernel.Application.DTOs;

/// <summary>
/// Resposta padrão da API
/// </summary>
/// <typeparam name="T">Tipo de dados retornado</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Sucesso { get; set; }

    /// <summary>
    /// Mensagem descritiva da operação
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Dados retornados pela operação
    /// </summary>
    public T? Dados { get; set; }

    /// <summary>
    /// Lista de erros (se houver)
    /// </summary>
    public List<string>? Erros { get; set; }

    /// <summary>
    /// Cria uma resposta de sucesso
    /// </summary>
    public static ApiResponse<T> Ok(T? dados, string mensagem = "Operação realizada com sucesso")
    {
        return new ApiResponse<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados
        };
    }


    /// <summary>
    /// Cria uma resposta de erro
    /// </summary>
    public static ApiResponse<T> Erro(string mensagem, List<string>? erros = null)
    {
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros
        };
    }

    /// <summary>
    /// Cria uma resposta de erro de validação
    /// </summary>
    public static ApiResponse<T> ErroValidacao(Dictionary<string, string[]> errosValidacao)
    {
        var erros = errosValidacao
            .SelectMany(kvp => kvp.Value.Select(erro => $"{kvp.Key}: {erro}"))
            .ToList();

        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = "Erro de validação",
            Erros = erros
        };
    }
}

