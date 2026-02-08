using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Domain.Interfaces;

namespace AgroSolutions.Identidade.Application.Services;

/// <summary>
/// Serviço de privacidade para conformidade com LGPD
/// </summary>
public class PrivacidadeService : IPrivacidadeService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PrivacidadeService> _logger;

    public PrivacidadeService(
        IUsuarioRepository usuarioRepository,
        IAuditoriaRepository auditoriaRepository,
        IAuditoriaService auditoriaService,
        IEmailService emailService,
        ILogger<PrivacidadeService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditoriaRepository = auditoriaRepository;
        _auditoriaService = auditoriaService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<DadosExportadosDto> ExportarDadosUsuarioAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        _logger.LogInformation("Exportando dados do usuário {UsuarioId}", usuarioId);

        // Buscar auditoria
        var auditorias = await _auditoriaRepository.ObterPorUsuarioAsync(usuarioId, cancellationToken);

        var data = new DadosExportadosDto
        {
            DataExportacao = DateTime.UtcNow,
            DadosPessoais = new
            {
                usuario.Id,
                usuario.NomeCompleto,
                usuario.Email,
                usuario.Telefone,
                usuario.Cpf,
                usuario.DataCriacao,
                usuario.DataUltimoAcesso
            },
            HistoricoAcessos = auditorias.Select(a => new AuditoriaDto(
                a.DataHora,
                a.Acao,
                a.Entidade,
                a.EnderecoIP,
                a.Sucesso
            )).ToList(),
            Consentimentos = new
            {
                TermosPrivacidade = new
                {
                    Aceito = true, // Sempre true se o usuário está ativo
                    DataAceite = usuario.DataCriacao,
                    Versao = "1.0"
                }
            }
        };

        await _auditoriaService.RegistrarAsync(
            acao: "EXPORTACAO_DADOS",
            entidade: "Usuario",
            entidadeId: usuarioId,
            sucesso: true
        );

        return data;
    }

    public async Task SolicitarExclusaoContaAsync(
        Guid usuarioId,
        string motivo,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        _logger.LogWarning("Usuário {UsuarioId} solicitou exclusão. Motivo: {Motivo}", usuarioId, motivo);

        // Marcar para exclusão (soft delete)
        usuario.MarcarParaExclusao(motivo);
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);

        // Registrar auditoria
        await _auditoriaService.RegistrarAsync(
            acao: "SOLICITACAO_EXCLUSAO",
            entidade: "Usuario",
            entidadeId: usuarioId,
            dadosNovos: new { Motivo = motivo },
            sucesso: true
        );

        // Enviar e-mail de confirmação
        try
        {
            await _emailService.EnviarEmailGenericoAsync(
                usuario.Email,
                "Solicitação de Exclusão de Conta - AgroSolutions",
                $@"
                <h2>Solicitação de Exclusão Recebida</h2>
                <p>Olá {usuario.NomeCompleto},</p>
                <p>Recebemos sua solicitação de exclusão de conta.</p>
                <p><strong>Prazo:</strong> Sua conta será excluída em até 30 dias conforme legislação (LGPD).</p>
                <p><strong>Motivo informado:</strong> {motivo}</p>
                <p>Se você não solicitou esta exclusão, entre em contato imediatamente conosco.</p>
                <br/>
                <p>Atenciosamente,<br/>Equipe AgroSolutions</p>
                ",
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail de confirmação de exclusão");
        }
    }

    public async Task<List<ConsentimentoDto>> ObterHistoricoConsentimentosAsync(
        Guid usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new InvalidOperationException("Usuário não encontrado");
        }

        var consentimentos = new List<ConsentimentoDto>
        {
            new ConsentimentoDto(
                Tipo: "Termos de Uso",
                Aceito: true,
                DataAceite: usuario.DataCriacao,
                Versao: "1.0"
            ),
            new ConsentimentoDto(
                Tipo: "Política de Privacidade",
                Aceito: true,
                DataAceite: usuario.DataCriacao,
                Versao: "1.0"
            )
        };

        return consentimentos;
    }
}
