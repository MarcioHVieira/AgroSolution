using Microsoft.AspNetCore.Builder;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Middleware para garantir UTF-8 em todas as respostas HTTP
/// Corrige problema de caracteres especiais no Scalar/Swagger
/// </summary>
public static class Utf8EncodingMiddleware
{
    public static IApplicationBuilder UseUtf8Encoding(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Força UTF-8 em todas as respostas
            context.Response.OnStarting(() =>
            {
                if (context.Response.ContentType != null && 
                    !context.Response.ContentType.Contains("charset", StringComparison.OrdinalIgnoreCase))
                {
                    context.Response.ContentType += "; charset=utf-8";
                }
                return Task.CompletedTask;
            });

            await next();
        });

        return app;
    }
}
