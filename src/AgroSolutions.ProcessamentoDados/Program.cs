namespace AgroSolutions.ProcessamentoDados;

using AgroSolutions.ProcessamentoDados.Configuration;
using AgroSolutions.ProcessamentoDados.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices<ProcessamentoDbContext>(builder.Configuration, builder.Environment);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        builder.Services.AddCorsConfiguration();

        var app = builder.Build();

        app.UseApiDocumentation();
        app.UseMonitoring();
        app.UseCorsConfiguration();
        app.UseStandardMicroservicePipeline<ProcessamentoDbContext>(serviceName: "AgroSolutions.ProcessamentoDados");

        app.Run();
    }
}