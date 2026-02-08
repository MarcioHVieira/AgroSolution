using AgroSolutions.SharedKernel.Application.DTOs;
using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Metrics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller responsável pela autenticação e gerenciamento de usuários
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly IIdentidadeService _identidadeServico;
    private readonly ILogger<AutenticacaoController> _logger;

    public AutenticacaoController(
        IIdentidadeService identidadeServico,
        ILogger<AutenticacaoController> logger)
    {
        _identidadeServico = identidadeServico;
        _logger = logger;
    }

    /// <summary>
    /// Registra um novo usuário no sistema
    /// </summary>
    /// <param name="dto">Dados do usuário a ser registrado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada com o resultado do registro</returns>
    [HttpPost("registrar")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<RegistroResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarUsuarioDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _identidadeServico.RegistrarUsuarioAsync(dto, cancellationToken);
        return Ok(ApiResponse<RegistroResponseDto>.Ok(resultado, "Usuário registrado com sucesso. Verifique seu e-mail para ativar a conta."));
    }

    /// <summary>
    /// Valida o código de ativação da conta
    /// </summary>
    /// <param name="dto">E-mail e código de validação</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("validar-codigo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidarCodigo(
        [FromBody] ValidarCodigoDto dto,
        CancellationToken cancellationToken)
    {
        await _identidadeServico.ValidarCodigoAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Código validado com sucesso. Sua conta foi ativada!"));
    }

    /// <summary>
    /// Reenvia o código de validação para o e-mail do usuário
    /// </summary>
    /// <param name="email">E-mail do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("reenviar-codigo")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReenviarCodigo(
        [FromQuery] string email,
        CancellationToken cancellationToken)
    {
        await _identidadeServico.ReenviarCodigoValidacaoAsync(email, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Código de validação reenviado com sucesso. Verifique seu e-mail."));
    }

    /// <summary>
    /// Solicita recuperação de senha
    /// </summary>
    /// <param name="dto">Email do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("esqueci-senha")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EsqueciSenha(
        [FromBody] EsqueciSenhaDto dto,
        CancellationToken cancellationToken)
    {
        await _identidadeServico.EsqueciSenhaAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Se o email existir, você receberá um código de recuperação."));
    }

    /// <summary>
    /// Redefine senha com código de recuperação
    /// </summary>
    /// <param name="dto">Email, código e nova senha</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("redefinir-senha")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedefinirSenha(
        [FromBody] RedefinirSenhaDto dto,
        CancellationToken cancellationToken)
    {
        await _identidadeServico.RedefinirSenhaAsync(dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Senha redefinida com sucesso! Faça login com sua nova senha."));
    }

    /// <summary>
    /// Altera senha do usuário autenticado
    /// </summary>
    /// <param name="dto">Senha atual e nova senha</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("alterar-senha")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha(
        [FromBody] AlterarSenhaDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("Usuário não autenticado"));

        await _identidadeServico.AlterarSenhaAsync(usuarioId, dto, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Senha alterada com sucesso!"));
    }

    /// <summary>
    /// Exporta todos os dados do usuário (LGPD - Direito à Portabilidade)
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Todos os dados pessoais do usuário em formato JSON</returns>
    [HttpGet("exportar-dados")]
    [Authorize]
    [EnableRateLimiting("lgpd")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportarDados(CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("Usuário não autenticado"));

        var data = await _identidadeServico.ExportarDadosUsuarioAsync(usuarioId, cancellationToken);
        return Ok(ApiResponse<object>.Ok(data, "Dados exportados com sucesso. Use este arquivo para portabilidade."));
    }

    /// <summary>
    /// Solicita exclusão de conta (LGPD - Direito ao Esquecimento)
    /// </summary>
    /// <param name="dto">Confirmação de senha para segurança</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta de confirmação</returns>
    [HttpDelete("excluir-conta")]
    [Authorize]
    [EnableRateLimiting("lgpd")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExcluirConta(
        [FromBody] ConfirmarSenhaDto dto,
        CancellationToken cancellationToken)
    {
        var usuarioId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
            ?? throw new UnauthorizedAccessException("Usuário não autenticado"));

        await _identidadeServico.ExcluirContaAsync(usuarioId, dto.Senha, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Sua conta foi marcada para exclusão. Todos os dados serão removidos em 30 dias conforme LGPD."));
    }

    /// <summary>
    /// Realiza o login do usuário
    /// </summary>
    /// <param name="dto">Credenciais de login</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada com o token JWT de acesso</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _identidadeServico.LoginAsync(dto, cancellationToken);
        return Ok(ApiResponse<TokenResponseDto>.Ok(resultado, "Login realizado com sucesso."));
    }

    /// <summary>
    /// Renova o access token usando refresh token
    /// </summary>
    /// <param name="dto">Refresh token</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Novo access token e refresh token</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var resultado = await _identidadeServico.RefreshTokenAsync(dto, cancellationToken);
        return Ok(ApiResponse<TokenResponseDto>.Ok(resultado, "Token renovado com sucesso."));
    }

    /// <summary>
    /// Revoga um refresh token (logout)
    /// </summary>
    /// <param name="dto">Refresh token a ser revogado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta padronizada</returns>
    [HttpPost("revogar-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevogarToken(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        await _identidadeServico.RevogarTokenAsync(dto.RefreshToken, cancellationToken);
        return Ok(ApiResponse<object>.Ok(dados: null, mensagem: "Token revogado com sucesso. Logout realizado."));
    }

    /// <summary>
    /// Endpoint de teste para verificar autenticação
    /// </summary>
    /// <returns>Informações do usuário autenticado</returns>
    [HttpGet("verificar-autenticacao")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public IActionResult VerificarAutenticacao()
    {
        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var perfil = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        var data = new
        {
            usuarioId,
            email,
            perfil
        };

        return Ok(ApiResponse<object>.Ok(data, "Autenticado com sucesso."));
    }
}
