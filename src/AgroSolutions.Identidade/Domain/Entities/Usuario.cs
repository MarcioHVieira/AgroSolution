using AgroSolutions.Identidade.Domain.Enums;

namespace AgroSolutions.Identidade.Domain.Entities;

/// <summary>
/// Entidade de domínio representando um Usuário do sistema
/// </summary>
public class Usuario
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;
    public string? Telefone { get; private set; }
    public string? Cpf { get; private set; }
    public PerfilAcesso Perfil { get; private set; }
    public StatusUsuario Status { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? DataUltimoAcesso { get; private set; }
    
    // Soft Delete (LGPD - Direito ao Esquecimento)
    public bool Excluido { get; private set; }
    public DateTime? DataExclusao { get; private set; }
    public string? MotivoExclusao { get; private set; }

    // Bloqueio de conta (segurança - proteção contra força bruta)
    public int TentativasFalhasLogin { get; private set; }
    public DateTime? DataUltimaTentativaFalha { get; private set; }
    public DateTime? DataBloqueio { get; private set; }
    public bool ContaBloqueada => DataBloqueio.HasValue && DataBloqueio.Value > DateTime.UtcNow;

    // Propriedade de navegação
    public ICollection<CodigoValidacao> CodigosValidacao { get; private set; } = new List<CodigoValidacao>();
    public ICollection<AuditoriaAcesso> Auditorias { get; private set; } = new List<AuditoriaAcesso>();

    // Construtor para o EF
    private Usuario() { }

    public Usuario(string nomeCompleto, string email, string senhaHash, PerfilAcesso perfil, string? telefone = null, string? cpf = null)
    {
        ValidarInvariantes(nomeCompleto, email, senhaHash);

        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto;
        Email = email.ToLowerInvariant();
        SenhaHash = senhaHash;
        Perfil = perfil;
        Telefone = telefone;
        Cpf = cpf;
        Status = StatusUsuario.AguardandoValidacao;
        DataCriacao = DateTime.UtcNow;
    }

    public void AtualizarSenha(string novaSenhaHash)
    {
        if (string.IsNullOrWhiteSpace(novaSenhaHash))
            throw new ArgumentException("Hash da senha não pode ser vazio", nameof(novaSenhaHash));

        SenhaHash = novaSenhaHash;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtivarConta()
    {
        Status = StatusUsuario.Ativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Bloquear()
    {
        Status = StatusUsuario.Bloqueado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desbloquear()
    {
        Status = StatusUsuario.Ativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void RegistrarAcesso()
    {
        DataUltimoAcesso = DateTime.UtcNow;
    }

    public void AtualizarPerfil(string nomeCompleto, string? telefone, string? cpf)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("Nome completo não pode ser vazio", nameof(nomeCompleto));

        NomeCompleto = nomeCompleto;
        Telefone = telefone;
        Cpf = cpf;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarParaExclusao(string motivo = "Solicitação do usuário - LGPD")
    {
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        MotivoExclusao = motivo;
        Status = StatusUsuario.Inativo;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Registra tentativa de login falha e bloqueia conta após 5 tentativas
    /// </summary>
    public void RegistrarTentativaFalhaLogin()
    {
        TentativasFalhasLogin++;
        DataUltimaTentativaFalha = DateTime.UtcNow;

        // Bloquear conta após 5 tentativas falhas
        if (TentativasFalhasLogin >= 5)
        {
            DataBloqueio = DateTime.UtcNow.AddMinutes(30); // Bloqueia por 30 minutos
        }

        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Reseta contador de tentativas falhas após login bem-sucedido
    /// </summary>
    public void ResetarTentativasFalhas()
    {
        TentativasFalhasLogin = 0;
        DataUltimaTentativaFalha = null;
        DataBloqueio = null;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Anonimiza dados pessoais conforme LGPD
    /// </summary>
    public void Anonimizar()
    {
        NomeCompleto = $"Usuário Anônimo {Id.ToString()[..8]}";
        Email = $"anonimo-{Id}@excluido.local";
        Telefone = null;
        Cpf = null;
        SenhaHash = string.Empty;
        Excluido = true;
        DataExclusao = DateTime.UtcNow;
        MotivoExclusao = "Anonimizado conforme LGPD";
        Status = StatusUsuario.Excluido;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Atualiza data do último acesso
    /// </summary>
    public void AtualizarUltimoAcesso()
    {
        DataUltimoAcesso = DateTime.UtcNow;
    }

    private static void ValidarInvariantes(string nomeCompleto, string email, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException("Nome completo não pode ser vazio", nameof(nomeCompleto));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("Hash da senha não pode ser vazio", nameof(senhaHash));
    }
}

