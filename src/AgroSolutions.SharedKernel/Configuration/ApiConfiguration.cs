using AgroSolutions.SharedKernel.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace AgroSolutions.SharedKernel.Configuration;

/// <summary>
/// Configuração unificada de API e serialização JSON para todos os microserviços
/// </summary>
public static class ApiConfiguration
{
    /// <summary>
    /// Adiciona configuração padrão de controllers e JSON serialization
    /// </summary>
    /// <param name="addApiResponseFilter">Se true, adiciona o filtro de padronização de resposta (padrão: true)</param>
    public static IServiceCollection AddStandardApiConfiguration(
        this IServiceCollection services, 
        bool addApiResponseFilter = true)
    {
        var mvcBuilder = services.AddControllers(options =>
        {
            // Adiciona o filtro de padronização de resposta se solicitado
            if (addApiResponseFilter)
            {
                options.Filters.Add<ApiResponseFilter>();
            }
        })
        .AddJsonOptions(options =>
        {
            ConfigureJsonOptions(options.JsonSerializerOptions);
        });

        services.AddEndpointsApiExplorer();

        return services;
    }

    /// <summary>
    /// Configura opções padrão de serialização JSON
    /// - UTF-8 sem escape de caracteres especiais
    /// - camelCase para propriedades
    /// - Ignora valores null
    /// - Indentação para melhor legibilidade
    /// </summary>
    public static void ConfigureJsonOptions(JsonSerializerOptions options)
    {
        // Suporte completo para UTF-8
        options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
        
        // Padrão camelCase para APIs REST
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        
        // Não serializa propriedades com valor null
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        
        // JSON indentado para facilitar debug
        options.WriteIndented = true;
        
        // Permite comentários em JSON
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        
        // Permite trailing commas
        options.AllowTrailingCommas = true;
    }
}
