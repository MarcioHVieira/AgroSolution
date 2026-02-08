namespace AgroSolutions.Propriedades.Domain.Entities;

/// <summary>
/// Read Model de Usuário sincronizado via eventos do microserviço Identidade
/// </summary>
public class UsuarioInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public DateTime DataSincronizacao { get; set; }
}
