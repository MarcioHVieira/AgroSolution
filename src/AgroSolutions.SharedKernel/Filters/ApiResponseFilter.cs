using AgroSolutions.SharedKernel.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgroSolutions.SharedKernel.Filters;

/// <summary>
/// Filtro unificado para padronizar respostas de sucesso da API em todos os microserviços
/// </summary>
public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Não faz nada antes da execução da ação
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Só processa se a resposta for bem-sucedida e for ObjectResult
        if (context.Exception == null && context.Result is ObjectResult objectResult)
        {
            // Se já é uma ApiResponse, não precisa encapsular novamente
            if (IsApiResponse(objectResult.Value))
            {
                return;
            }

            // Encapsula a resposta em ApiResponse padrão
            var response = ApiResponse<object>.Ok(
                objectResult.Value, 
                "Operação realizada com sucesso");

            context.Result = new ObjectResult(response)
            {
                StatusCode = objectResult.StatusCode ?? 200
            };
        }
    }

    /// <summary>
    /// Verifica se o valor já é uma ApiResponse
    /// </summary>
    private static bool IsApiResponse(object? value)
    {
        if (value == null)
            return false;

        var type = value.GetType();

        // Verifica se é ApiResponse genérico
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            return true;
        }

        // Verifica se é ApiResponse<object>
        if (type == typeof(ApiResponse<object>))
        {
            return true;
        }

        return false;
    }
}
