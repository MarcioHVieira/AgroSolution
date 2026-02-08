namespace AgroSolutions.Sensores;

using AgroSolutions.Sensores.Configuration;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices(builder.Configuration, builder.Environment, requiresAuth: false);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        
        AgroSolutions.Sensores.Configuration.CorsConfiguration.AddCorsConfiguration(builder.Services);

        var app = builder.Build();

        app.UseUtf8Encoding();
        app.UseExceptionHandling();
        app.UseApiDocumentation();
        app.UseMonitoring();
        
        AgroSolutions.Sensores.Configuration.CorsConfiguration.UseCorsConfiguration(app);
        
        app.MapControllers();

        app.Run();
    }
}