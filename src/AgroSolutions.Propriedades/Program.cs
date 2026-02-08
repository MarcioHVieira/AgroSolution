namespace AgroSolutions.Propriedades;

using AgroSolutions.Propriedades.Configuration;
using AgroSolutions.Propriedades.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices<PropriedadesDbContext>(builder.Configuration, builder.Environment);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        builder.Services.AddCorsConfiguration();

        var app = builder.Build();

        app.UseApiDocumentation();
        app.UseMonitoring();
        app.UseCorsConfiguration();
        app.UseStandardMicroservicePipeline<PropriedadesDbContext>(serviceName: "AgroSolutions.Propriedades");

        app.Run();
    }
}