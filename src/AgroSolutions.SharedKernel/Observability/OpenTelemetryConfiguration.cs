using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AgroSolutions.SharedKernel.Observability;

/// <summary>
/// Configuração centralizada de OpenTelemetry para todos os microsserviços
/// </summary>
public static class OpenTelemetryConfiguration
{
    /// <summary>
    /// Configura OpenTelemetry com Jaeger para um microsserviço
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <param name="serviceName">Nome do microsserviço</param>
    public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource("AgroSolutions")
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName)
                            .AddAttributes(new Dictionary<string, object>
                            {
                                ["service.namespace"] = "AgroSolutions",
                                ["deployment.environment"] = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development"
                            }))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = (httpContext) =>
                        {
                            // Não rastrear health checks
                            return !httpContext.Request.Path.StartsWithSegments("/health");
                        };
                    })
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.SetDbStatementForStoredProcedure = true;
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                    });

                if (configuration["Observability:UseOtlp"]?.ToLower() == "true")
                {
                    var otlpEndpoint = configuration["Observability:OtlpEndpoint"] ?? "http://localhost:4317";
                    
                    tracerProviderBuilder.AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                }

                // Adicionar exportador Console em desenvolvimento
                if (configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
                {
                    tracerProviderBuilder.AddConsoleExporter();
                }
            });

        return services;
    }

    /// <summary>
    /// Cria um ActivitySource para tracing personalizado
    /// </summary>
    public static System.Diagnostics.ActivitySource CreateActivitySource()
    {
        return new System.Diagnostics.ActivitySource("AgroSolutions");
    }
}
