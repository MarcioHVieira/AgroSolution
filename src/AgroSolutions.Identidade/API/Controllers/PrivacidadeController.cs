using System.Text;
using System.Text.Json;
using AgroSolutions.Identidade.API.Extensions;
using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller de privacidade e conformidade LGPD
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Tags("Privacidade")]
public class PrivacidadeController : ControllerBase
{
    private readonly IPrivacidadeService _privacidadeService;
    private readonly ILogger<PrivacidadeController> _logger;

    public PrivacidadeController(
        IPrivacidadeService privacidadeService,
        ILogger<PrivacidadeController> logger)
    {
        _privacidadeService = privacidadeService;
        _logger = logger;
    }

    /// <summary>
    /// Exporta todos os dados pessoais do usuário (LGPD Art. 18, IV - Portabilidade)
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Arquivo JSON com todos os dados do usuário</returns>
    [HttpGet("exportar-dados")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportarDados(CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        _logger.LogInformation("Usuário {UsuarioId} solicitou exportação de dados", usuarioId);

        var dadosExportados = await _privacidadeService.ExportarDadosUsuarioAsync(usuarioId, cancellationToken);

        var json = JsonSerializer.Serialize(dadosExportados, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var bytes = Encoding.UTF8.GetBytes(json);
        var fileName = $"meus_dados_agrosolutions_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

        return File(bytes, "application/json", fileName);
    }

    /// <summary>
    /// Solicita exclusão de conta (LGPD Art. 18, VI - Direito ao Esquecimento)
    /// </summary>
    /// <param name="dto">Dados da solicitação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Confirmação da solicitação</returns>
    [HttpPost("solicitar-exclusao")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SolicitarExclusao(
        [FromBody] SolicitarExclusaoDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();

        _logger.LogWarning("Usuário {UsuarioId} solicitou exclusão de conta. Motivo: {Motivo}",
            usuarioId, dto.Motivo);

        await _privacidadeService.SolicitarExclusaoContaAsync(usuarioId, dto.Motivo, cancellationToken);

        return Ok(ApiResponse<object>.Ok(
            "Solicitação de exclusão registrada com sucesso. " +
            "Sua conta será excluída em até 30 dias conforme legislação (LGPD Art. 16). " +
            "Um e-mail de confirmação foi enviado."));
    }

    /// <summary>
    /// Retorna histórico de consentimentos do usuário (LGPD Art. 18, VIII)
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de consentimentos</returns>
    [HttpGet("historico-consentimentos")]
    [ProducesResponseType(typeof(ApiResponse<List<ConsentimentoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterHistoricoConsentimentos(CancellationToken cancellationToken)
    {
        var usuarioId = User.ObterUsuarioId();
        var historico = await _privacidadeService.ObterHistoricoConsentimentosAsync(usuarioId, cancellationToken);

        return Ok(ApiResponse<List<ConsentimentoDto>>.Ok(historico, "Histórico de consentimentos obtido com sucesso"));
    }

    /// <summary>
    /// Retorna a política de privacidade (LGPD Art. 9)
    /// </summary>
    /// <returns>Texto da política de privacidade</returns>
    [HttpGet("politica-privacidade")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult ObterPoliticaPrivacidade()
    {
        var politica = new
        {
            Versao = "1.0",
            DataAtualizacao = new DateTime(2025, 1, 1),
            Conteudo = @"
POLÍTICA DE PRIVACIDADE - AGROSOLUTIONS

1. COLETA DE DADOS
Coletamos apenas dados essenciais para o funcionamento da plataforma:
- Dados cadastrais: nome completo, e-mail, telefone (opcional), CPF (opcional)
- Dados de acesso: endereço IP, user agent, data/hora de acesso
- Dados agrícolas: propriedades, talhões, leituras de sensores

2. USO DE DADOS
Seus dados são utilizados exclusivamente para:
- Autenticação e autorização no sistema
- Fornecimento dos serviços de agricultura de precisão
- Envio de alertas e notificações relacionados às suas propriedades
- Melhoria contínua da plataforma

3. COMPARTILHAMENTO
NãO compartilhamos seus dados pessoais com terceiros sem seu consentimento expresso.

4. DIREITOS DO TITULAR (LGPD Art. 18)
Você tem direito a:
- Acessar seus dados (endpoint: GET /api/privacidade/exportar-dados)
- Corrigir dados incorretos (endpoint: PUT /api/autenticacao/atualizar-perfil)
- Solicitar exclusão (endpoint: POST /api/privacidade/solicitar-exclusao)
- Revogar consentimento
- Portabilidade de dados

5. SEGURANÇA
Utilizamos as seguintes medidas de segurança:
- Criptografia Argon2id para senhas
- JWT com RSA 2048 bits
- HTTPS obrigatório
- Rate limiting
- Auditoria de acessos
- Logs sanitizados (sem dados sensíveis)

6. RETENÇÃO DE DADOS
- Dados de usuários ativos: mantidos enquanto a conta estiver ativa
- Dados de usuários inativos: excluídos após 30 dias da solicitação
- Dados estatísticos: anonimizados e mantidos para fins analíticos

7. COOKIES
Não utilizamos cookies de terceiros. Apenas sessão JWT para autenticação.

8. CONTATO
Encarregado de Dados (DPO): privacidade@agrosolutions.com.br

Última atualização: 01/01/2025
            "
        };

        return Ok(ApiResponse<object>.Ok(politica, "Política de privacidade"));
    }
}
