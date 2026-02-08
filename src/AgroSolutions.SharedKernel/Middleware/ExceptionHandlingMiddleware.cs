using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.SharedKernel.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AgroSolutions.SharedKernel.Middleware;

/// <summary>
/// Middleware unificado para tratamento global de exceções em todos os microserviços
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, errorDetails) = MapExceptionToErrorDetails(exception, context.TraceIdentifier);
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var json = JsonSerializer.Serialize(errorDetails, options);
        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode statusCode, ErrorDetails errorDetails) MapExceptionToErrorDetails(
        Exception exception, 
        string? traceId)
    {
        HttpStatusCode statusCode;
        string message;
        string? code = null;

        switch (exception)
        {
            // Exceções do SharedKernel
            case ValidationException validationEx:
                statusCode = HttpStatusCode.BadRequest;
                message = validationEx.Message;
                code = "VALIDATION_ERROR";
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                message = notFoundEx.Message;
                code = "NOT_FOUND";
                break;

            case BusinessException businessEx:
                statusCode = HttpStatusCode.BadRequest;
                message = businessEx.Message;
                code = "BUSINESS_ERROR";
                break;

            // Exceções .NET padrão
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Forbidden;
                message = "Acesso negado.";
                code = "FORBIDDEN";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Recurso não encontrado.";
                code = "NOT_FOUND";
                break;

            case ArgumentNullException or ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Requisição inválida. Verifique os parâmetros.";
                code = "BAD_REQUEST";
                break;

            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                message = "Operação inválida.";
                code = "INVALID_OPERATION";
                break;

            // Exceção genérica
            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "Ocorreu um erro interno no servidor.";
                code = "INTERNAL_ERROR";
                break;
        }

        var errorDetails = new ErrorDetails
        {
            StatusCode = (int)statusCode,
            Code = code,
            Type = exception.GetType().Name,
            Message = message,
            TraceId = traceId,
            Timestamp = DateTime.UtcNow
        };

        // Em desenvolvimento, inclui detalhes completos
        if (_environment.IsDevelopment())
        {
            errorDetails.Details = exception.Message;
            errorDetails.StackTrace = exception.StackTrace;
        }

        return (statusCode, errorDetails);
    }
}
