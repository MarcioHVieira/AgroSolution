using AgroSolutions.Identidade.Application.Interfaces;

namespace AgroSolutions.Identidade.Infrastructure.BackgroundJobs;

/// <summary>
/// Background service para exclusão automática de contas conforme LGPD
/// Executa diariamente às 02:00 AM
/// </summary>
public class ExclusaoAutomaticaJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExclusaoAutomaticaJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Executa a cada 24 horas

    public ExclusaoAutomaticaJob(
        IServiceProvider serviceProvider,
        ILogger<ExclusaoAutomaticaJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExclusaoAutomaticaJob iniciado. Próxima execução: {ProximaExecucao}", ObterProximaExecucao());

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var agora = DateTime.Now;
                var proximaExecucao = ObterProximaExecucao();
                var delay = proximaExecucao - agora;

                if (delay.TotalMilliseconds > 0)
                {
                    _logger.LogInformation("Aguardando até {ProximaExecucao} para executar job de exclusão automática", proximaExecucao);
                    await Task.Delay(delay, stoppingToken);
                }

                await ProcessarExclusoesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no job de exclusão automática: {Message}", ex.Message);
            }
        }
    }

    private async Task ProcessarExclusoesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando job de exclusão automática de contas (LGPD)");

        var startTime = DateTime.UtcNow;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var anonimizacaoService = scope.ServiceProvider.GetRequiredService<IAnonimizacaoService>();

            int usuariosAnonimizados = await anonimizacaoService.ProcessarExclusoesAutomaticasAsync(cancellationToken);

            var duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Job de exclusão automática concluído. " +
                "Usuários anonimizados: {Count}. Duração: {Duration}s",
                usuariosAnonimizados,
                duration.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no job de exclusão automática: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Calcula a próxima execução às 02:00 AM
    /// </summary>
    private static DateTime ObterProximaExecucao()
    {
        var agora = DateTime.Now;
        var proximaExecucao = agora.Date.AddHours(2); // 02:00 AM

        // Se já passou das 02:00 hoje, agendar para amanhã
        if (agora > proximaExecucao)
        {
            proximaExecucao = proximaExecucao.AddDays(1);
        }

        return proximaExecucao;
    }
}
