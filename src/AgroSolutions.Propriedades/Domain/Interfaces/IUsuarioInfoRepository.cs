namespace AgroSolutions.Propriedades.Domain.Interfaces;

/// <summary>
/// Repository para acesso ao Read Model de informações de usuários (sincronizado via eventos)
/// </summary>
public interface IUsuarioInfoRepository
{
    /// <summary>
    /// Obtém informações de um usuário por ID
    /// </summary>
    /// <param name="usuarioId">ID do usuário</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Informações do usuário ou null se não encontrado</returns>
    Task<(string Email, string NomeCompleto)?> ObterDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
