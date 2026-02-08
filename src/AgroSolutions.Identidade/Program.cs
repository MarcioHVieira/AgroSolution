namespace AgroSolutions.Identidade;

using AgroSolutions.Identidade.Configuration;
using AgroSolutions.Identidade.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices<IdentidadeDbContext>(builder.Configuration, builder.Environment);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        builder.Services.AddCorsConfiguration();

        var app = builder.Build();

        app.UseApiDocumentation();
        app.UseMonitoring();
        app.UseCorsConfiguration();
        app.UseStandardMicroservicePipeline<IdentidadeDbContext>(serviceName: "AgroSolutions.Identidade");

        app.Run();
    }
}