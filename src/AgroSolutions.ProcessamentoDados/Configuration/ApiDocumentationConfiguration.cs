using System.Text.Encodings.Web;
using System.Text.Unicode;
using Scalar.AspNetCore;

namespace AgroSolutions.ProcessamentoDados.Configuration;

public static class ApiDocumentationConfiguration
{
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            options.SerializerOptions.WriteIndented = true;
        });
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, token) =>
            {
                document.Info.Title = "AgroSolutions - API de Processamento";
                document.Info.Version = "v1.0";
                document.Info.Description = """
                    ## Microserviço de Processamento de Dados
                    Este microserviço é responsável pelo **processamento e análise** de leituras de sensores.
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
                .WithTitle("AgroSolutions.ProcessamentoDados API")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithSidebar(true);
        });

        app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

        return app;
    }
}
