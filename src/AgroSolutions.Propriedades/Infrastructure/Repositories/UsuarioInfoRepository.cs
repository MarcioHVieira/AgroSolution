using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Propriedades.Infrastructure.Repositories;

/// <summary>
/// Repository para acesso ao Read Model de informações de usuários
/// </summary>
public class UsuarioInfoRepository : IUsuarioInfoRepository
{
    private readonly PropriedadesDbContext _context;

    public UsuarioInfoRepository(PropriedadesDbContext context)
    {
        _context = context;
    }

    public async Task<(string Email, string NomeCompleto)?> ObterDadosUsuarioAsync(
        Guid usuarioId, 
        CancellationToken cancellationToken = default)
    {
        var usuario = await _context.UsuariosInfo
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);

        if (usuario == null)
        {
            return null;
        }

        return (usuario.Email, usuario.NomeCompleto);
    }
}
