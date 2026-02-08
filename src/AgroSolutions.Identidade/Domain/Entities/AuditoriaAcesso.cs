namespace AgroSolutions.Identidade.Domain.Entities;

/// <summary>
/// Entidade de auditoria para registrar todas as ações importantes no sistema
/// Compliance: LGPD, ISO 27001
/// </summary>
public class AuditoriaAcesso
{
    public Guid Id { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public string Acao { get; private set; } = string.Empty;
    public string Entidade { get; private set; } = string.Empty;
    public Guid? EntidadeId { get; private set; }
    public string? DadosAntigos { get; private set; }
    public string? DadosNovos { get; private set; }
    public string EnderecoIP { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public DateTime DataHora { get; private set; }
    public bool Sucesso { get; private set; }
    public string? MensagemErro { get; private set; }

    // Navegação
    public Usuario? Usuario { get; private set; }

    // Construtor privado para EF Core
    private AuditoriaAcesso() { }

    public AuditoriaAcesso(
        Guid? usuarioId,
        string acao,
        string entidade,
        Guid? entidadeId,
        string enderecoIP,
        bool sucesso,
        string? dadosAntigos = null,
        string? dadosNovos = null,
        string? userAgent = null,
        string? mensagemErro = null)
    {
        if (string.IsNullOrWhiteSpace(acao))
            throw new ArgumentException("Ação é obrigatória", nameof(acao));

        if (string.IsNullOrWhiteSpace(entidade))
            throw new ArgumentException("Entidade é obrigatória", nameof(entidade));

        if (string.IsNullOrWhiteSpace(enderecoIP))
            throw new ArgumentException("Endereço IP é obrigatório", nameof(enderecoIP));

        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Acao = acao;
        Entidade = entidade;
        EntidadeId = entidadeId;
        EnderecoIP = enderecoIP;
        UserAgent = userAgent;
        DataHora = DateTime.UtcNow;
        Sucesso = sucesso;
        DadosAntigos = dadosAntigos;
        DadosNovos = dadosNovos;
        MensagemErro = mensagemErro;
    }
}
