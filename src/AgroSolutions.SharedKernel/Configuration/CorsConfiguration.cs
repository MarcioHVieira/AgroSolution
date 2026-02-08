using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configuração unificada de CORS para todos os microserviços
/// </summary>
public static class CorsConfiguration
{
    private const string AllowAllPolicyName = "AllowAll";

    /// <summary>
    /// Adiciona política CORS que permite qualquer origem, método e header
    /// ATENÇÃO: Esta configuração é permissiva e deve ser ajustada para produção
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(AllowAllPolicyName, policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }

    /// <summary>
    /// Usa a política CORS configurada
    /// </summary>
    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors(AllowAllPolicyName);
        return app;
    }
}
