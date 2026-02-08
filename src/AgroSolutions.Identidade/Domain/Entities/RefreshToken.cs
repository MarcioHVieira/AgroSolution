namespace AgroSolutions.Identidade.Domain.Entities;

/// <summary>
/// Representa um refresh token para renovação de JWT
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Token { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime DataExpiracao { get; private set; }
    public bool Revogado { get; private set; }
    public DateTime? DataRevogacao { get; private set; }
    public string? MotivoRevogacao { get; private set; }
    public string? SubstituidoPor { get; private set; }
    public string? IpAddress { get; private set; }

    // EF Core
    private RefreshToken() 
    { 
        Token = string.Empty;
    }

    public RefreshToken(
        Guid usuarioId,
        string token,
        DateTime dataExpiracao,
        string? ipAddress = null)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        DataCriacao = DateTime.UtcNow;
        DataExpiracao = dataExpiracao;
        Revogado = false;
        IpAddress = ipAddress;
    }

    public bool EstaValido()
    {
        return !Revogado && DateTime.UtcNow < DataExpiracao;
    }

    public bool EstaExpirado()
    {
        return DateTime.UtcNow >= DataExpiracao;
    }

    public void Revogar(string motivo, string? substituidoPor = null)
    {
        Revogado = true;
        DataRevogacao = DateTime.UtcNow;
        MotivoRevogacao = motivo;
        SubstituidoPor = substituidoPor;
    }
}
