namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface para geração de tokens JWT
/// </summary>
public interface ITokenService
{
    string GerarToken(Guid usuarioId, string email, string perfil);
    string GerarRefreshToken();
}
