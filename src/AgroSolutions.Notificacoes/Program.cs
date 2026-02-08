namespace AgroSolutions.Notificacoes;

using AgroSolutions.Notificacoes.Configuration;
using AgroSolutions.Notificacoes.Infrastructure.Data;
using AgroSolutions.SharedKernel.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddStandardMicroserviceServices<NotificacoesDbContext>(builder.Configuration, builder.Environment);
        builder.Services.AddApiDocumentation();
        builder.Services.AddMonitoring();
        builder.Services.AddCorsConfiguration();

        var app = builder.Build();

        app.UseApiDocumentation();
        app.UseMonitoring();
        app.UseCorsConfiguration();
        app.UseStandardMicroservicePipeline<NotificacoesDbContext>(serviceName: "AgroSolutions.Notificacoes");

        app.Run();
    }
}