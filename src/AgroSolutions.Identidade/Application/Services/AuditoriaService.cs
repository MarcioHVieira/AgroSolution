using System.Text.Json;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Interfaces;

namespace AgroSolutions.Identidade.Application.Services;

/// <summary>
/// Serviço de auditoria para registrar ações no sistema
/// </summary>
public class AuditoriaService : IAuditoriaService
{
    private readonly IAuditoriaRepository _auditoriaRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditoriaService> _logger;

    public AuditoriaService(
        IAuditoriaRepository auditoriaRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditoriaService> logger)
    {
        _auditoriaRepository = auditoriaRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task RegistrarAsync(
        string acao,
        string entidade,
        Guid? entidadeId = null,
        object? dadosAntigos = null,
        object? dadosNovos = null,
        bool sucesso = true,
        string? mensagemErro = null)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var usuarioId = httpContext?.User?.FindFirst("sub")?.Value;
            var enderecoIP = httpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

            // Serializar dados de forma segura
            string? dadosAntigosJson = null;
            string? dadosNovosJson = null;

            if (dadosAntigos != null)
            {
                try
                {
                    dadosAntigosJson = JsonSerializer.Serialize(dadosAntigos);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao serializar dados antigos para auditoria");
                }
            }

            if (dadosNovos != null)
            {
                try
                {
                    dadosNovosJson = JsonSerializer.Serialize(dadosNovos);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao serializar dados novos para auditoria");
                }
            }

            var auditoria = new AuditoriaAcesso(
                usuarioId != null ? Guid.Parse(usuarioId) : null,
                acao,
                entidade,
                entidadeId,
                enderecoIP,
                sucesso,
                dadosAntigosJson,
                dadosNovosJson,
                userAgent,
                mensagemErro
            );

            await _auditoriaRepository.AdicionarAsync(auditoria);

            _logger.LogInformation(
                "Auditoria registrada: {Acao} em {Entidade} por usuário {UsuarioId} - Sucesso: {Sucesso}",
                acao, entidade, usuarioId ?? "anônimo", sucesso);
        }
        catch (Exception ex)
        {
            // Não propagar exceçõo para não quebrar o fluxo principal
            _logger.LogError(ex, "Erro ao registrar auditoria: {Message}", ex.Message);
        }
    }
}
