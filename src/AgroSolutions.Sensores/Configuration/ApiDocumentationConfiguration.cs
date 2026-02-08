using System.Text.Encodings.Web;
using System.Text.Unicode;
using Scalar.AspNetCore;

namespace AgroSolutions.Sensores.Configuration;

public static class ApiDocumentationConfiguration
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        // Configurar JSON para UTF-8 sem escapar caracteres Unicode
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.SerializerOptions.WriteIndented = true;
        });
        
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, token) =>
            {
                document.Info.Title = "AgroSolutions - Simulador de Sensores";
                document.Info.Version = "v1.0";
                document.Info.Description = """
                    ## Simulador de Sensores IoT para Agricultura de Precisão
                    
                    Este microserviço permite **simular cenários realistas de sensores** instalados em talhões agrícolas, 
                    gerando dados que disparam todo o fluxo do sistema AgroSolutions:
                    """;
                document.Info.Contact = new()
                {
                    Name = "Equipe AgroSolutions",
                    Email = "suporte@agrosolutions.com"
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("AgroSolutions.Sensores API")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithSidebar(true);
        });

        // Redirecionar raiz para o Scalar
        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

        return app;
    }
}
