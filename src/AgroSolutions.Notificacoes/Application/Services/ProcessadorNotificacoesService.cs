using AgroSolutions.Notificacoes.Application.Interfaces;
using AgroSolutions.Notificacoes.Domain.Enums;
using AgroSolutions.Notificacoes.Domain.Interfaces;

namespace AgroSolutions.Notificacoes.Application.Services;

public class ProcessadorNotificacoesService : IProcessadorNotificacoesService
{
    private readonly INotificacaoRepository _repository;
    private readonly IEmailService _emailService;
    private readonly INotificacaoService _notificacaoService;
    private readonly ILogger<ProcessadorNotificacoesService> _logger;

    public ProcessadorNotificacoesService(
        INotificacaoRepository repository,
        IEmailService emailService,
        INotificacaoService notificacaoService,
        ILogger<ProcessadorNotificacoesService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _notificacaoService = notificacaoService;
        _logger = logger;
    }

    public async Task ProcessarNotificacoesPendentesAsync()
    {
        var pendentes = await _repository.ObterPendentesAsync();

        foreach (var notificacao in pendentes)
        {
            try
            {
                bool enviada = false;
                string? mensagemErro = null;

                if (notificacao.Tipo == TipoNotificacao.Email)
                {
                    enviada = await _emailService.EnviarEmailAsync(
                        notificacao.EmailDestinatario,
                        notificacao.Assunto,
                        notificacao.Mensagem
                    );

                    if (!enviada)
                        mensagemErro = "Falha no envio de email";
                }

                // Marca como enviada e publica evento
                await _notificacaoService.MarcarComoEnviadaAsync(notificacao.Id, enviada, mensagemErro);

                if (!enviada)
                {
                    notificacao.TentativasEnvio++;
                    notificacao.Status = notificacao.TentativasEnvio >= 3 
                        ? StatusNotificacao.Falha 
                        : StatusNotificacao.Reenviando;
                    await _repository.AtualizarAsync(notificacao);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar notificação {NotificacaoId}", notificacao.Id);
                await _notificacaoService.MarcarComoEnviadaAsync(notificacao.Id, false, ex.Message);
                
                notificacao.TentativasEnvio++;
                notificacao.Status = StatusNotificacao.Falha;
                notificacao.MensagemErro = ex.Message;
                await _repository.AtualizarAsync(notificacao);
            }
        }
    }
}
