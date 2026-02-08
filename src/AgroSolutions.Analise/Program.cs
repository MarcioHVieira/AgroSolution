namespace AgroSolutions.Analise;

using AgroSolutions.Analise.Configuration;
using AgroSolutions.Analise.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices<AnaliseDbContext>(builder.Configuration, builder.Environment);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        builder.Services.AddCorsConfiguration();

        var app = builder.Build();

        app.UseApiDocumentation();
        app.UseMonitoring();
        app.UseCorsConfiguration();
        app.UseStandardMicroservicePipeline<AnaliseDbContext>(serviceName: "AgroSolutions.Analise");

        app.Run();
    }
}