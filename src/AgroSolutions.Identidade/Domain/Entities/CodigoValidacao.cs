namespace AgroSolutions.Identidade.Domain.Entities;

/// <summary>
/// Entidade de domónio representando um Código de Validação de E-mail
/// </summary>
public class CodigoValidacao
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public DateTime DataCriacao { get; private set; }
    public DateTime DataExpiracao { get; private set; }
    public bool Utilizado { get; private set; }
    public DateTime? DataUtilizacao { get; private set; }

    // Propriedade de navegação
    public Usuario Usuario { get; private set; } = null!;

    // Construtor para o EF
    private CodigoValidacao() { }

    public CodigoValidacao(Guid usuarioId, string codigo, int minutosValidade = 30)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Codigo = codigo;
        DataCriacao = DateTime.UtcNow;
        DataExpiracao = DateTime.UtcNow.AddMinutes(minutosValidade);
        Utilizado = false;
    }

    public bool EstaValido()
    {
        return !Utilizado && DateTime.UtcNow <= DataExpiracao;
    }

    public void MarcarComoUtilizado()
    {
        Utilizado = true;
        DataUtilizacao = DateTime.UtcNow;
    }

    public bool EstaExpirado()
    {
        return DateTime.UtcNow > DataExpiracao;
    }
}
