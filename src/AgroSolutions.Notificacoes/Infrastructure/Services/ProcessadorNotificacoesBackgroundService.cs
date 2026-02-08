using AgroSolutions.Notificacoes.Application.Interfaces;

namespace AgroSolutions.Notificacoes.Infrastructure.Services;

/// <summary>
/// Background Service que processa notificações pendentes periodicamente
/// </summary>
public class ProcessadorNotificacoesBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessadorNotificacoesBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public ProcessadorNotificacoesBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ProcessadorNotificacoesBackgroundService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Aguarda 10 segundos antes de iniciar
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        _logger.LogInformation("Processador de Notificações iniciado");

        // Intervalo de processamento (padrão: 30 segundos)
        var intervalo = TimeSpan.FromSeconds(
            _configuration.GetValue<int>("NotificacoesSettings:IntervaloProcessamentoSegundos", 30));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processador = scope.ServiceProvider.GetRequiredService<IProcessadorNotificacoesService>();

                _logger.LogDebug("Processando notificações pendentes...");
                await processador.ProcessarNotificacoesPendentesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificações pendentes");
            }

            await Task.Delay(intervalo, stoppingToken);
        }

        _logger.LogInformation("Processador de Notificações encerrado");
    }
}
