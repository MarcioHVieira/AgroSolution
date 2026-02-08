using System.Security.Claims;

namespace AgroSolutions.Propriedades.API.Extensions;

/// <summary>
/// Extensões para obter informações do usuário autenticado
/// </summary>
public static class UserContextExtensions
{
    /// <summary>
    /// Obtém o ID do usuário autenticado
    /// </summary>
    public static Guid ObterUsuarioId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("Usuário não autenticado");

        return Guid.Parse(userIdClaim);
    }

    /// <summary>
    /// Obtém o perfil do usuário autenticado
    /// </summary>
    public static string ObterPerfil(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value
            ?? throw new UnauthorizedAccessException("Perfil do usuário não encontrado");
    }

    /// <summary>
    /// Verifica se o usuário é administrador
    /// </summary>
    public static bool EhAdministrador(this ClaimsPrincipal user)
    {
        var perfil = user.FindFirst(ClaimTypes.Role)?.Value;
        return perfil?.Equals("Administrador", StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Verifica se o usuário é proprietário de um recurso
    /// </summary>
    public static bool EhProprietarioOuAdmin(this ClaimsPrincipal user, Guid proprietarioId)
    {
        if (user.EhAdministrador())
            return true;

        var usuarioId = user.ObterUsuarioId();
        return usuarioId == proprietarioId;
    }
}
