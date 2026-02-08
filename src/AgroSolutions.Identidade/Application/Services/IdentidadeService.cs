using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Events;
using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Enums;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.SharedKernel.Messaging;

namespace AgroSolutions.Identidade.Application.Services;

public class IdentidadeService : IIdentidadeService
{
    private readonly IUsuarioRepository _usuarioRepositorio;
    private readonly ICodigoValidacaoRepository _codigoValidacaoRepositorio;
    private readonly IRefreshTokenRepository _refreshTokenRepositorio;
    private readonly ICriptografiaService _criptografiaServico;
    private readonly IEmailService _emailServico;
    private readonly ITokenService _tokenServico;
    private readonly ILogger<IdentidadeService> _logger;
    private readonly IRabbitMQPublisher _publisher;

    public IdentidadeService(
        IUsuarioRepository usuarioRepositorio,
        ICodigoValidacaoRepository codigoValidacaoRepositorio,
        IRefreshTokenRepository refreshTokenRepositorio,
        ICriptografiaService criptografiaServico,
        IEmailService emailServico,
        ITokenService tokenServico,
        ILogger<IdentidadeService> logger,
        IRabbitMQPublisher publisher)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _codigoValidacaoRepositorio = codigoValidacaoRepositorio;
        _refreshTokenRepositorio = refreshTokenRepositorio;
        _criptografiaServico = criptografiaServico;
        _emailServico = emailServico;
        _tokenServico = tokenServico;
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<RegistroResponseDto> RegistrarUsuarioAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (await _usuarioRepositorio.ExisteEmailAsync(dto.Email, cancellationToken))
        {
            throw new InvalidOperationException("E-mail já cadastrado no sistema");
        }

        if (!string.IsNullOrEmpty(dto.Cpf) && await _usuarioRepositorio.ExisteCpfAsync(dto.Cpf, cancellationToken))
        {
            throw new InvalidOperationException("CPF já cadastrado no sistema");
        }

        var senhaHash = _criptografiaServico.GerarHash(dto.Senha);

        var usuario = new Usuario(
            dto.NomeCompleto,
            dto.Email,
            senhaHash,
            PerfilAcesso.Usuario,
            dto.Telefone,
            dto.Cpf
        );

        await _usuarioRepositorio.AdicionarAsync(usuario, cancellationToken);

        var codigoValidacao = GerarCodigoValidacao();
        var codigo = new CodigoValidacao(usuario.Id, codigoValidacao);

        await _codigoValidacaoRepositorio.AdicionarAsync(codigo, cancellationToken);

        await _emailServico.EnviarEmailValidacaoAsync(
            usuario.Email,
            usuario.NomeCompleto,
            codigoValidacao,
            cancellationToken
        );

        _logger.LogInformation("Usuário {Email} registrado. Código de validação enviado", usuario.Email);

        // Publicar evento de usuário criado
        await _publisher.PublishAsync(new UsuarioCriadoEvent(
            Id: usuario.Id,
            Email: usuario.Email,
            NomeCompleto: usuario.NomeCompleto,
            DataCriacao: usuario.DataCriacao
        ), "usuario.criado");

        return new RegistroResponseDto(usuario.Id);
    }

    public async Task ValidarCodigoAsync(ValidarCodigoDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuario == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }

        if (usuario.Status == StatusUsuario.Ativo)
        {
            throw new InvalidOperationException("Conta já está ativa");
        }

        var codigoValidacao = await _codigoValidacaoRepositorio.ObterPorCodigoAsync(dto.Codigo, cancellationToken);
        if (codigoValidacao == null || codigoValidacao.UsuarioId != usuario.Id)
        {
            throw new ArgumentException("Código de validação inválido");
        }

        if (!codigoValidacao.EstaValido())
        {
            throw new ArgumentException(codigoValidacao.EstaExpirado() 
                ? "Código de validação expirado" 
                : "Código de validação já foi utilizado");
        }

        usuario.AtivarConta();
        codigoValidacao.MarcarComoUtilizado();

        await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);
        await _codigoValidacaoRepositorio.AtualizarAsync(codigoValidacao, cancellationToken);

        _logger.LogInformation("Conta do usuário {Email} ativada", usuario.Email);

        // Publicar evento de atualização (usuário ativado)
        await PublicarEventoUsuarioAtualizadoAsync(usuario);
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        if (!_criptografiaServico.VerificarSenha(dto.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        if (!usuario.SenhaHash.StartsWith("$argon2id$"))
        {
            var novoHash = _criptografiaServico.GerarHash(dto.Senha);
            usuario.AtualizarSenha(novoHash);
            await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);
            _logger.LogInformation("Hash de senha migrado para Argon2: {Email}", dto.Email);
        }

        if (usuario.Status == StatusUsuario.AguardandoValidacao)
        {
            throw new UnauthorizedAccessException("Conta ainda não foi validada. Verifique seu e-mail.");
        }

        if (usuario.Status == StatusUsuario.Bloqueado)
        {
            throw new UnauthorizedAccessException("Conta bloqueada. Entre em contato com o suporte.");
        }

        if (usuario.Status == StatusUsuario.Inativo)
        {
            throw new UnauthorizedAccessException("Conta inativa. Entre em contato com o suporte.");
        }

        var token = _tokenServico.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString());

        usuario.RegistrarAcesso();
        await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);

        _logger.LogInformation("Login realizado para {Email}", usuario.Email);

        var usuarioDto = new UsuarioDto(
            usuario.Id,
            usuario.NomeCompleto,
            usuario.Email,
            usuario.Telefone,
            usuario.Cpf,
            usuario.Perfil.ToString(),
            usuario.Status.ToString(),
            usuario.DataCriacao
        );

        // Gera refresh token
        var refreshToken = _tokenServico.GerarRefreshToken();
        var refreshTokenEntity = new RefreshToken(
            usuario.Id,
            refreshToken,
            DateTime.UtcNow.AddDays(7) // Expira em 7 dias
        );

        // Revoga refresh tokens antigos do usuário
        await _refreshTokenRepositorio.RevogarTodosDoUsuarioAsync(usuario.Id, "Novo login realizado", cancellationToken);

        // Salva o novo refresh token
        await _refreshTokenRepositorio.AdicionarAsync(refreshTokenEntity, cancellationToken);

        return new TokenResponseDto(
            token,
            refreshToken,
            "Bearer",
            3600,
            usuarioDto
        );
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenRepositorio.ObterPorTokenAsync(dto.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido");
        }

        if (!refreshToken.EstaValido())
        {
            throw new UnauthorizedAccessException(refreshToken.EstaExpirado()
                ? "Refresh token expirado"
                : "Refresh token revogado");
        }

        var usuario = await _usuarioRepositorio.ObterPorIdAsync(refreshToken.UsuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado");
        }

        if (usuario.Status != StatusUsuario.Ativo)
        {
            throw new UnauthorizedAccessException("Usuário inativo");
        }

        // Gera novo access token
        var novoAccessToken = _tokenServico.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString());

        // Gera novo refresh token
        var novoRefreshToken = _tokenServico.GerarRefreshToken();
        var novoRefreshTokenEntity = new RefreshToken(
            usuario.Id,
            novoRefreshToken,
            DateTime.UtcNow.AddDays(7)
        );

        // Revoga o refresh token antigo
        refreshToken.Revogar("Substituído por novo refresh token", novoRefreshToken);
        await _refreshTokenRepositorio.AtualizarAsync(refreshToken, cancellationToken);

        // Salva o novo refresh token
        await _refreshTokenRepositorio.AdicionarAsync(novoRefreshTokenEntity, cancellationToken);

        var usuarioDto = new UsuarioDto(
            usuario.Id,
            usuario.NomeCompleto,
            usuario.Email,
            usuario.Telefone,
            usuario.Cpf,
            usuario.Perfil.ToString(),
            usuario.Status.ToString(),
            usuario.DataCriacao
        );

        return new TokenResponseDto(
            novoAccessToken,
            novoRefreshToken,
            "Bearer",
            3600,
            usuarioDto
        );
    }

    public async Task RevogarTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepositorio.ObterPorTokenAsync(refreshToken, cancellationToken);

        if (token == null)
        {
            throw new ArgumentException("Refresh token não encontrado");
        }

        if (token.Revogado)
        {
            throw new ArgumentException("Refresh token já foi revogado");
        }

        token.Revogar("Revogado manualmente pelo usuário");
        await _refreshTokenRepositorio.AtualizarAsync(token, cancellationToken);
    }

    public async Task ReenviarCodigoValidacaoAsync(string email, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorEmailAsync(email, cancellationToken);
        if (usuario == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }

        if (usuario.Status == StatusUsuario.Ativo)
        {
            throw new InvalidOperationException("Conta já está ativa");
        }

        var codigoValidacao = GerarCodigoValidacao();
        var codigo = new CodigoValidacao(usuario.Id, codigoValidacao);

        await _codigoValidacaoRepositorio.AdicionarAsync(codigo, cancellationToken);

        await _emailServico.EnviarEmailValidacaoAsync(
            usuario.Email,
            usuario.NomeCompleto,
            codigoValidacao,
            cancellationToken
        );
    }

    public async Task EsqueciSenhaAsync(EsqueciSenhaDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuario == null)
        {
            _logger.LogWarning("Tentativa de recuperação de senha para email não cadastrado: {Email}", dto.Email);
            return;
        }

        var codigoRecuperacao = GerarCodigoValidacao();
        var codigo = new CodigoValidacao(usuario.Id, codigoRecuperacao);
        
        await _codigoValidacaoRepositorio.AdicionarAsync(codigo, cancellationToken);

        await _emailServico.EnviarEmailRecuperacaoSenhaAsync(
            usuario.Email,
            usuario.NomeCompleto,
            codigoRecuperacao,
            cancellationToken
        );
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuario == null)
        {
            throw new ArgumentException("Email não encontrado");
        }

        var codigoValidacao = await _codigoValidacaoRepositorio.ObterPorCodigoAsync(dto.Codigo, cancellationToken);
        if (codigoValidacao == null || codigoValidacao.UsuarioId != usuario.Id)
        {
            throw new ArgumentException("Código de recuperação inválido");
        }

        if (!codigoValidacao.EstaValido())
        {
            throw new ArgumentException(codigoValidacao.EstaExpirado() 
                ? "Código de recuperação expirado" 
                : "Código de recuperação já foi utilizado");
        }

        var novaSenhaHash = _criptografiaServico.GerarHash(dto.NovaSenha);
        
        usuario.AtualizarSenha(novaSenhaHash);
        codigoValidacao.MarcarComoUtilizado();

        await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);
        await _codigoValidacaoRepositorio.AtualizarAsync(codigoValidacao, cancellationToken);
    }

    public async Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto dto, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new UnauthorizedAccessException("Usuário não encontrado");
        }

        if (!_criptografiaServico.VerificarSenha(dto.SenhaAtual, usuario.SenhaHash))
        {
            throw new ArgumentException("Senha atual incorreta");
        }

        if (dto.SenhaAtual == dto.NovaSenha)
        {
            throw new ArgumentException("A nova senha deve ser diferente da senha atual");
        }

        var novaSenhaHash = _criptografiaServico.GerarHash(dto.NovaSenha);
        
        usuario.AtualizarSenha(novaSenhaHash);
        await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);

        // Publicar evento de atualização
        await PublicarEventoUsuarioAtualizadoAsync(usuario);
    }

    public async Task<object> ExportarDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        _logger.LogInformation("Exportando dados do usuário {UsuarioId} conforme LGPD", usuarioId);

        // Exporta todos os dados pessoais do usuário
        var dadosExportados = new
        {
            DadosPessoais = new
            {
                usuario.Id,
                usuario.NomeCompleto,
                usuario.Email,
                usuario.Telefone,
                usuario.Cpf,
                Perfil = usuario.Perfil.ToString(),
                Status = usuario.Status.ToString()
            },
            Metadados = new
            {
                usuario.DataCriacao,
                usuario.DataAtualizacao,
                usuario.DataUltimoAcesso
            },
            InformacoesLGPD = new
            {
                DataExportacao = DateTime.UtcNow,
                Finalidade = "Portabilidade de dados conforme Art. 18, V da LGPD",
                Observacao = "Este arquivo contém todos os seus dados pessoais armazenados em nosso sistema."
            }
        };

        return dadosExportados;
    }

    public async Task ExcluirContaAsync(Guid usuarioId, string senha, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepositorio.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Verifica senha para confirmar identidade
        if (!_criptografiaServico.VerificarSenha(senha, usuario.SenhaHash))
        {
            throw new UnauthorizedAccessException("Senha incorreta. Não foi possível confirmar a exclusão.");
        }

        _logger.LogWarning("Usuário {UsuarioId} ({Email}) solicitou exclusão de conta - LGPD Art. 18, VI", 
            usuarioId, usuario.Email);

        // Soft delete - marca para exclusão (dados serão removidos após 30 dias)
        usuario.MarcarParaExclusao("Solicitação do usuário - LGPD Art. 18, VI (Direito ao Esquecimento)");
        await _usuarioRepositorio.AtualizarAsync(usuario, cancellationToken);

        // Revoga todos os tokens ativos
        var tokens = await _refreshTokenRepositorio.ObterTodosPorUsuarioIdAsync(usuarioId, cancellationToken);
        foreach (var token in tokens.Where(t => !t.Revogado))
        {
            token.Revogar("Exclusão de conta - LGPD");
            await _refreshTokenRepositorio.AtualizarAsync(token, cancellationToken);
        }

        // Envia e-mail de confirmação
        try
        {
            await _emailServico.EnviarEmailExclusaoContaAsync(
                usuario.Email, 
                usuario.NomeCompleto, 
                DateTime.UtcNow.AddDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar e-mail de confirmação de exclusão para {Email}", usuario.Email);
            // Não falha a operação se o e-mail não for enviado
        }
    }

    private static string GerarCodigoValidacao()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Publica evento de usuário atualizado
    /// Chamado sempre que dados importantes do usuário são modificados
    /// </summary>
    private async Task PublicarEventoUsuarioAtualizadoAsync(Usuario usuario)
    {
        try
        {
            await _publisher.PublishAsync(new UsuarioAtualizadoEvent(
                Id: usuario.Id,
                Email: usuario.Email,
                NomeCompleto: usuario.NomeCompleto,
                DataAtualizacao: usuario.DataAtualizacao ?? DateTime.UtcNow
            ), "usuario.atualizado");
        }
        catch (Exception ex)
        {
            // Log mas não falha a operação principal
            _logger.LogError(ex, "Erro ao publicar evento de usuário atualizado: {UsuarioId}", usuario.Id);
        }
    }
}
