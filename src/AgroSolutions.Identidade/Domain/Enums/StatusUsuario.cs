namespace AgroSolutions.Identidade.Domain.Enums;

/// <summary>
/// Status do usuário no sistema
/// </summary>
public enum StatusUsuario
{
    /// <summary>
    /// Usuário aguardando validação do e-mail
    /// </summary>
    AguardandoValidacao = 1,
    
    /// <summary>
    /// Usuário ativo no sistema
    /// </summary>
    Ativo = 2,
    
    /// <summary>
    /// Usuário bloqueado
    /// </summary>
    Bloqueado = 3,
    
    /// <summary>
    /// Usuário inativo
    /// </summary>
    Inativo = 4,
    
    /// <summary>
    /// Usuário excluído/anonimizado (LGPD)
    /// </summary>
    Excluido = 5
}
