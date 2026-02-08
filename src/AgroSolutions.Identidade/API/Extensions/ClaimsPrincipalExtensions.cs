using System.Security.Claims;

namespace AgroSolutions.Identidade.API.Extensions;

/// <summary>
/// Métodos de extensão para ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Obtém o ID do usuário a partir das claims
    /// </summary>
    public static Guid ObterUsuarioId(this ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? user.FindFirst("sub") 
                          ?? user.FindFirst("userId");

        if (userIdClaim == null)
            throw new InvalidOperationException("Usuário não autenticado ou claim de ID não encontrada");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            throw new InvalidOperationException("ID de usuário inválido");

        return userId;
    }

    /// <summary>
    /// Obtém o e-mail do usuário a partir das claims
    /// </summary>
    public static string? ObterEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
               ?? user.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Obtém o nome do usuário a partir das claims
    /// </summary>
    public static string? ObterNome(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value 
               ?? user.FindFirst("name")?.Value;
    }
}
